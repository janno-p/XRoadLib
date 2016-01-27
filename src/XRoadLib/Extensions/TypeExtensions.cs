using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using XRoadLib.Attributes;
using XRoadLib.Schema;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions
{
    public delegate object GetValueMethod(object source);
    public delegate void SetValueMethod(object source, object value);

    public static class TypeExtensions
    {
        public static bool IsAnonymous(this Type type)
        {
            return (type.GetSingleAttribute<XmlTypeAttribute>()?.AnonymousType).GetValueOrDefault();
        }

        public static bool IsXRoadSerializable(this Type type)
        {
            var baseType = type.BaseType;

            while (baseType != null)
            {
                if (baseType == typeof(XRoadSerializable))
                    return true;
                baseType = baseType.BaseType;
            }

            return false;
        }

        public static IEnumerable<PropertyInfo> GetPropertiesSorted(this Type type, IComparer<PropertyInfo> comparer, uint? version)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            var properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                 .Where(prop => (!prop.Name.Contains(".") || prop.GetSingleAttribute<XRoadRemoveContractAttribute>() != null) &&
                                                (!version.HasValue || prop.ExistsInVersion(version.Value)));

            return new SortedSet<PropertyInfo>(properties, comparer);
        }

        public static IEnumerable<PropertyInfo> GetAllPropertiesSorted(this Type type, IComparer<PropertyInfo> comparer, uint? version)
        {
            var properties = new List<PropertyInfo>();

            if (type.BaseType != typeof(XRoadSerializable))
                properties.AddRange(type.BaseType.GetAllPropertiesSorted(comparer, version));

            properties.AddRange(type.GetPropertiesSorted(comparer, version));

            return properties;
        }

        public static IEnumerable<Tuple<string, string>> GetXRoadTitles(this ICustomAttributeProvider attributeProvider)
        {
            return attributeProvider.GetCustomAttributes(typeof(XRoadTitleAttribute), false)
                                    .OfType<XRoadTitleAttribute>()
                                    .Select(x => Tuple.Create(x.LanguageCode, x.Value));
        }

        public static string GetElementName(this ICustomAttributeProvider attributeProvider)
        {
            return (attributeProvider.GetSingleAttribute<XmlElementAttribute>()?.ElementName).GetValueOrDefault();
        }

        public static bool ExistsInVersion(this ICustomAttributeProvider provider, uint version)
        {
            return IsVersionInRange(
                version,
                provider.GetSingleAttribute<XRoadAddContractAttribute>()?.Version,
                provider.GetSingleAttribute<XRoadRemoveContractAttribute>()?.Version
                );
        }

        public static bool ExistsInVersion(this XRoadServiceAttribute attribute, uint version)
        {
            return IsVersionInRange(version, attribute.addedInVersion, attribute.removedInVersion);
        }

        public static bool HasMultipartRequest(this MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttributes(typeof(XRoadAttachmentAttribute), false)
                             .OfType<XRoadAttachmentAttribute>()
                             .Select(x => x.HasMultipartRequest).SingleOrDefault();
        }

        public static bool HasMultipartResponse(this MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttributes(typeof(XRoadAttachmentAttribute), false)
                             .OfType<XRoadAttachmentAttribute>()
                             .Select(x => x.HasMultipartResponse).SingleOrDefault();
        }

        public static IEnumerable<string> GetServicesInVersion(this MethodInfo methodInfo, uint version, bool includeHidden = false)
        {
            return methodInfo.GetCustomAttributes(typeof(XRoadServiceAttribute), false)
                             .OfType<XRoadServiceAttribute>()
                             .Where(x => includeHidden || !x.IsHidden)
                             .Where(x => IsVersionInRange(version, x.addedInVersion, x.removedInVersion))
                             .Select(x => x.Name)
                             .ToList();
        }

        internal static bool IsVersionInRange(uint version, uint? versionAdded, uint? versionRemoved)
        {
            return version >= versionAdded.GetValueOrDefault() && version < versionRemoved.GetValueOrDefault(uint.MaxValue);
        }

        internal static XName GetSystemTypeName(this Type type)
        {
            if (type == typeof(bool)) return XName.Get("boolean", NamespaceConstants.XSD);
            if (type == typeof(DateTime)) return XName.Get("dateTime", NamespaceConstants.XSD);
            if (type == typeof(decimal)) return XName.Get("decimal", NamespaceConstants.XSD);
            if (type == typeof(int)) return XName.Get("int", NamespaceConstants.XSD);
            if (type == typeof(long)) return XName.Get("long", NamespaceConstants.XSD);
            if (type == typeof(string)) return XName.Get("string", NamespaceConstants.XSD);
            return typeof(Stream).IsAssignableFrom(type) ? XName.Get("base64Binary", NamespaceConstants.XSD) : null;
        }

        public static T GetSingleAttribute<T>(this ICustomAttributeProvider customAttributeProvider)
        {
            return customAttributeProvider.GetCustomAttributes(typeof(T), false).Cast<T>().SingleOrDefault();
        }

        public static bool IsNullable(this Type type)
        {
            return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsFilterableField(this Type runtimeType, string fieldName)
        {
            return runtimeType.Assembly
                              .GetTypes()
                              .Where(t => typeof(IXRoadFilterMap).IsAssignableFrom(t))
                              .Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericArguments().Single().IsAssignableFrom(runtimeType))
                              .Select(t => (IXRoadFilterMap)Activator.CreateInstance(t))
                              .Where(m => m.GroupName.Equals("AvalikudAndmed"))
                              .Any(m => m.EnabledProperties.Contains(fieldName));
        }

        public static Tuple<MethodInfo, XRoadServiceAttribute> FindMethodDeclaration(this MethodInfo method, string operationName, IDictionary<MethodInfo, IDictionary<string, XRoadServiceAttribute>> serviceContracts)
        {
            if (method.DeclaringType == null)
                return null;

            var methodContracts = method.DeclaringType
                                       .GetInterfaces()
                                       .Select(iface => method.DeclaringType.GetInterfaceMap(iface))
                                       .Where(map => map.TargetMethods.Contains(method))
                                       .Select(map => map.InterfaceMethods[Array.IndexOf(map.TargetMethods, method)])
                                       .ToList();

            if (methodContracts.Count > 1)
                throw XRoadException.AmbiguousMatch(operationName);

            var methodContract = methodContracts.SingleOrDefault();
            IDictionary<string, XRoadServiceAttribute> serviceContract;

            if (methodContract == null || !serviceContracts.TryGetValue(methodContract, out serviceContract))
                throw XRoadException.UndefinedContract(operationName);

            XRoadServiceAttribute serviceAttribute;
            if (!serviceContract.TryGetValue(operationName, out serviceAttribute))
                throw XRoadException.UndefinedContract(operationName);

            return Tuple.Create(methodContract, serviceAttribute);
        }

        internal static string GetValueOrDefault(this string value, string defaultValue = null)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private static XName GetQualifiedDataType(string dataType)
        {
            return string.IsNullOrWhiteSpace(dataType)
                ? null
                : XName.Get(dataType, dataType == "base64" || dataType == "hexBinary" ? NamespaceConstants.SOAP_ENC : NamespaceConstants.XSD);
        }

        internal static XName GetQualifiedArrayItemDataType(this ICustomAttributeProvider provider)
        {
            return GetQualifiedDataType(provider.GetSingleAttribute<XmlArrayItemAttribute>()?.DataType);
        }

        internal static XName GetQualifiedElementDataType(this ICustomAttributeProvider provider)
        {
            return GetQualifiedDataType(provider.GetSingleAttribute<XmlElementAttribute>()?.DataType);
        }

        internal static bool IsRequiredElement(this ICustomAttributeProvider provider)
        {
            return provider.GetSingleAttribute<XRoadRequiredAttribute>() != null;
        }
    }
}