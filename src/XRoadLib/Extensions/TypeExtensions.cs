using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using System.Xml.Serialization;
using XRoadLib.Attributes;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions
{
    public delegate object GetValueMethod(object source);
    public delegate void SetValueMethod(object source, object value);

    public static class TypeExtensions
    {
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

        public static IEnumerable<PropertyInfo> GetPublicPropertiesSorted(this Type type)
        {
            return type.GetProperties().OrderBy(propertyInfo => propertyInfo.MetadataToken);
        }

        public static IEnumerable<PropertyInfo> GetPropertiesSorted<T>(this Type type, uint dtoVersion, Func<PropertyInfo, T> orderBySelector)
        {
            return type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                       .Where(prop => (!prop.Name.Contains(".") || prop.GetSingleAttribute<XRoadRemoveContractAttribute>() != null) && prop.ExistsInVersion(dtoVersion))
                       .OrderBy(orderBySelector);
        }

        public static IEnumerable<PropertyInfo> GetAllPropertiesSorted<T>(this Type type, uint dtoVersion, Func<PropertyInfo, T> orderBySelector)
        {
            var properties = new List<PropertyInfo>();

            if (type.BaseType != typeof(XRoadSerializable))
                properties.AddRange(type.BaseType.GetAllPropertiesSorted(dtoVersion, orderBySelector));

            properties.AddRange(type.GetPropertiesSorted(dtoVersion, orderBySelector));

            return properties;
        }

        public static IEnumerable<PropertyInfo> GetAllPropertiesSorted(this Type type, uint dtoVersion, XRoadProtocol protocol)
        {
            return protocol == XRoadProtocol.Version20 ? type.GetAllPropertiesSorted(dtoVersion, p => p.MetadataToken) : type.GetAllPropertiesSorted(dtoVersion, p => p.GetPropertyName());
        }

        public static IEnumerable<Tuple<string, string>> GetXRoadTitles(this ICustomAttributeProvider attributeProvider)
        {
            return attributeProvider.GetCustomAttributes(typeof(XRoadTitleAttribute), false)
                                    .OfType<XRoadTitleAttribute>()
                                    .Select(x => Tuple.Create(x.LanguageCode, x.Value));
        }

        public static string GetElementName(this ICustomAttributeProvider attributeProvider)
        {
            var elementAttribute = attributeProvider.GetSingleAttribute<XmlElementAttribute>();
            return !string.IsNullOrWhiteSpace(elementAttribute?.ElementName) ? elementAttribute.ElementName : null;
        }

        public static string GetElementType(this ICustomAttributeProvider attributeProvider)
        {
            return attributeProvider.GetCustomAttributes(typeof(XmlElementAttribute), false)
                                    .OfType<XmlElementAttribute>()
                                    .Select(a => a.DataType)
                                    .SingleOrDefault();
        }

        public static bool ExistsInVersion(this ICustomAttributeProvider type, uint version)
        {
            var versionAdded = type.GetCustomAttributes(typeof(XRoadAddContractAttribute), false)
                                   .OfType<XRoadAddContractAttribute>()
                                   .Select(x => (uint?)x.Version)
                                   .SingleOrDefault();

            var versionRemoved = type.GetCustomAttributes(typeof(XRoadRemoveContractAttribute), false)
                                     .OfType<XRoadRemoveContractAttribute>()
                                     .Select(x => (uint?)x.Version)
                                     .SingleOrDefault();

            return IsVersionInRange(version, versionAdded, versionRemoved);
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
                             .Where(x => x.IsDefinedInVersion(version))
                             .Select(x => x.Name)
                             .ToList();
        }

        public static bool IsParameterInVersion(this ParameterInfo parameter, uint version)
        {
            var attribute = parameter.GetCustomAttributes(typeof(XRoadParameterAttribute), false)
                                      .OfType<XRoadParameterAttribute>()
                                      .SingleOrDefault();

            return attribute == null || attribute.IsDefinedInVersion(version);
        }

        public static bool IsDefinedInVersion<T>(this T instance, uint version) where T : IXRoadLifetime
        {
            return IsVersionInRange(version, instance.AddedInVersion, instance.RemovedInVersion);
        }

        private static bool IsVersionInRange(uint version, uint? versionAdded, uint? versionRemoved)
        {
            return version >= versionAdded.GetValueOrDefault() && version < versionRemoved.GetValueOrDefault(uint.MaxValue);
        }

        private static string GetFixedPropertyName(this PropertyInfo propertyInfo)
        {
            var nameStart = propertyInfo.Name.LastIndexOf('.');
            return nameStart >= 0 ? propertyInfo.Name.Substring(nameStart + 1) : propertyInfo.Name;
        }

        public static string GetTypeName(this Type type)
        {
            return type.Name;
        }

        public static string GetPropertyName(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetElementName() ?? propertyInfo.GetFixedPropertyName();
        }

        public static bool IsImportedOperation(this MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttributes(typeof(XRoadImportAttribute), false).Any();
        }

        public static Tuple<XmlQualifiedName, XmlQualifiedName> GetImportedOperationTypeNames(this MethodInfo methodInfo, string @namespace)
        {
            return methodInfo.GetCustomAttributes(typeof(XRoadImportAttribute), false)
                             .OfType<XRoadImportAttribute>()
                             .Select(x => Tuple.Create(new XmlQualifiedName(x.RequestPart, @namespace), new XmlQualifiedName(x.ResponsePart, @namespace)))
                             .SingleOrDefault();
        }

        public static IEnumerable<XRoadMessagePartAttribute> GetExtraMessageParts(this MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttributes(typeof(XRoadMessagePartAttribute), false)
                             .OfType<XRoadMessagePartAttribute>();
        }

        public static XmlQualifiedName ToQualifiedName(this Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            return nullableType != null ? new XmlQualifiedName(nullableType.Name + "?", type.Namespace) : new XmlQualifiedName(type.Name, type.Namespace);
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

        public static GetValueMethod CreateGetValueMethod(this PropertyInfo propertyInfo)
        {
            var type = propertyInfo.DeclaringType;
            if (type == null)
                return o => null;

            var converterAttribute = propertyInfo.GetSingleAttribute<XRoadRemoveContractAttribute>();
            var converterType = converterAttribute?.Converter;

            var propertyType = propertyInfo.PropertyType;

            var dynamicSet = new DynamicMethod("DynamicGet", typeof(object), new[] { typeof(object) }, type, true);
            var generator = dynamicSet.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);

            if (converterType != null)
                generator.Emit(OpCodes.Call, converterType.GetMethod("ConvertBack", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
            else
                generator.Emit(OpCodes.Callvirt, propertyInfo.GetGetMethod(true));

            if (propertyType.IsValueType)
                generator.Emit(OpCodes.Box, propertyType);

            generator.Emit(OpCodes.Ret);

            return (GetValueMethod)dynamicSet.CreateDelegate(typeof(GetValueMethod));
        }

        public static SetValueMethod CreateSetValueMethod(this PropertyInfo propertyInfo)
        {
            var type = propertyInfo.DeclaringType;
            if (type == null)
                return (o, v) => { };

            var converterAttribute = propertyInfo.GetSingleAttribute<XRoadRemoveContractAttribute>();
            var converterType = converterAttribute?.Converter;

            var propertyType = propertyInfo.PropertyType;

            var dynamicSet = new DynamicMethod("DynamicSet", typeof(void), new[] { typeof(object), typeof(object) }, type, true);
            var generator = dynamicSet.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);

            if (propertyType.IsValueType)
                generator.Emit(OpCodes.Unbox_Any, propertyType);

            if (converterType != null)
                generator.Emit(OpCodes.Call, converterType.GetMethod("Convert", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
            else
                generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod(true));

            generator.Emit(OpCodes.Ret);

            return (SetValueMethod)dynamicSet.CreateDelegate(typeof(SetValueMethod));
        }
    }
}