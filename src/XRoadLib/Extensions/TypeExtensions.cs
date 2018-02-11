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
            return (type.GetTypeInfo().GetCustomAttribute<XmlTypeAttribute>()?.AnonymousType).GetValueOrDefault();
        }

        public static bool IsXRoadSerializable(this Type type)
        {
            var baseType = type.GetTypeInfo().BaseType;

            while (baseType != null)
            {
                if (baseType == typeof(XRoadSerializable))
                    return true;
                baseType = baseType.GetTypeInfo().BaseType;
            }

            return false;
        }

        public static IEnumerable<PropertyDefinition> GetPropertiesSorted(this Type type, IComparer<PropertyDefinition> comparer, uint? version, Func<PropertyInfo, PropertyDefinition> createDefinition)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            var properties = type.GetTypeInfo()
                                 .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                 .Where(prop => !prop.Name.Contains(".") || prop.GetSingleAttribute<XRoadRemoveContractAttribute>() != null)
                                 .Where(prop => !version.HasValue || prop.ExistsInVersion(version.Value))
                                 .Select(createDefinition);

            return new SortedSet<PropertyDefinition>(properties, comparer);
        }

        public static IEnumerable<PropertyDefinition> GetAllPropertiesSorted(this Type type, IComparer<PropertyDefinition> comparer, uint? version, Func<PropertyInfo, PropertyDefinition> createDefinition)
        {
            var properties = new List<PropertyDefinition>();

            if (type.GetTypeInfo().BaseType != typeof(XRoadSerializable))
                properties.AddRange(type.GetTypeInfo().BaseType.GetAllPropertiesSorted(comparer, version, createDefinition));

            properties.AddRange(type.GetPropertiesSorted(comparer, version, createDefinition));

            return properties;
        }

        public static IEnumerable<XRoadTitleAttribute> GetXRoadTitles(this ICustomAttributeProvider customAttributeProvider)
        {
            return customAttributeProvider.GetCustomAttributes(typeof(XRoadTitleAttribute), false)
                                          .OfType<XRoadTitleAttribute>()
                                          .Where(x => !string.IsNullOrWhiteSpace(x.Value));
        }

        public static IEnumerable<XRoadNotesAttribute> GetXRoadNotes(this ICustomAttributeProvider customAttributeProvider)
        {
            return customAttributeProvider.GetCustomAttributes(typeof(XRoadNotesAttribute), false)
                                          .OfType<XRoadNotesAttribute>()
                                          .Where(x => !string.IsNullOrWhiteSpace(x.Value));
        }

        public static IEnumerable<XRoadTechNotesAttribute> GetXRoadTechNotes(this ICustomAttributeProvider customAttributeProvider)
        {
            return customAttributeProvider.GetCustomAttributes(typeof(XRoadTechNotesAttribute), false)
                                          .OfType<XRoadTechNotesAttribute>()
                                          .Where(x => !string.IsNullOrWhiteSpace(x.Value));
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

        public static IEnumerable<string> GetServicesInVersion(this MethodInfo methodInfo, uint version, bool includeHidden = false)
        {
            return methodInfo.GetCustomAttributes(typeof(XRoadServiceAttribute), false)
                             .OfType<XRoadServiceAttribute>()
                             .Where(x => includeHidden || !x.IsHidden)
                             .Where(x => IsVersionInRange(version, x.addedInVersion, x.removedInVersion))
                             .Select(x => x.Name)
                             .ToList();
        }

        public static IEnumerable<XRoadServiceAttribute> GetServices(this MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttributes(typeof(XRoadServiceAttribute), false)
                             .OfType<XRoadServiceAttribute>()
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
            return typeof(Stream).GetTypeInfo().IsAssignableFrom(type) ? XName.Get("base64Binary", NamespaceConstants.XSD) : null;
        }

        public static T GetSingleAttribute<T>(this ICustomAttributeProvider customAttributeProvider)
        {
            return customAttributeProvider.GetCustomAttributes(typeof(T), false).Cast<T>().SingleOrDefault();
        }

        public static bool IsNullable(this Type type)
        {
            return type != null && type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static bool BaseTypeHasGenericArgument(Type type, Type runtimeType)
        {
            var typeInfo = type.GetTypeInfo().BaseType?.GetTypeInfo();
            if (typeInfo == null)
                return false;

            return typeInfo.IsGenericType && typeInfo.GetGenericArguments().Single().GetTypeInfo().IsAssignableFrom(runtimeType);
        }

        public static bool IsFilterableField(this Type runtimeType, string fieldName, string groupName)
        {
            return runtimeType.GetTypeInfo()
                              .Assembly
                              .GetTypes()
                              .Where(t => typeof(IXRoadFilterMap).GetTypeInfo().IsAssignableFrom(t))
                              .Where(t => BaseTypeHasGenericArgument(t, runtimeType))
                              .Select(t => (IXRoadFilterMap)Activator.CreateInstance(t))
                              .Where(m => m.GroupName.Equals(groupName))
                              .Any(m => m.EnabledProperties.Contains(fieldName));
        }

        public static Tuple<MethodInfo, XRoadServiceAttribute> FindMethodDeclaration(this MethodInfo method, string operationName, IDictionary<MethodInfo, IDictionary<string, XRoadServiceAttribute>> serviceContracts)
        {
            if (method.DeclaringType == null)
                return null;

            var methodContracts = method.DeclaringType
                                        .GetTypeInfo()
                                        .GetInterfaces()
                                        .Select(iface => method.DeclaringType.GetTypeInfo().GetRuntimeInterfaceMap(iface))
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

        internal static XmlElementAttribute GetElementAttributeFromInterface(this Type declaringType, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                return null;

            var getMethod = propertyInfo.GetGetMethod();

            foreach (var iface in declaringType.GetTypeInfo().GetInterfaces())
            {
                var map = declaringType.GetTypeInfo().GetRuntimeInterfaceMap(iface);

                var index = -1;
                for (var i = 0; i < map.TargetMethods.Length; i++)
                    if (map.TargetMethods[i] == getMethod)
                    {
                        index = i;
                        break;
                    }

                if (index < 0)
                    continue;

                var ifaceProperty = iface.GetTypeInfo().GetProperties().SingleOrDefault(p => p.GetGetMethod() == map.InterfaceMethods[index]);

                var attribute = ifaceProperty.GetSingleAttribute<XmlElementAttribute>();
                if (attribute != null)
                    return attribute;
            }

            return null;
        }

        internal static Type NormalizeType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }
    }
}