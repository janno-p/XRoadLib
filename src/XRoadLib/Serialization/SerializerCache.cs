using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Protocols;
using XRoadLib.Schema;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Serialization
{
    public sealed class SerializerCache : ISerializerCache
    {
        private readonly Assembly contractAssembly;
        private readonly IProtocol protocol;

        private readonly ConcurrentDictionary<XName, IServiceMap> serviceMaps = new ConcurrentDictionary<XName, IServiceMap>();
        private readonly ConcurrentDictionary<XName, Tuple<ITypeMap, ITypeMap>> xmlTypeMaps = new ConcurrentDictionary<XName, Tuple<ITypeMap, ITypeMap>>();
        private readonly ConcurrentDictionary<Type, ITypeMap> runtimeTypeMaps = new ConcurrentDictionary<Type, ITypeMap>();

        public SerializerCache(Assembly contractAssembly, IProtocol protocol)
        {
            this.contractAssembly = contractAssembly;
            this.protocol = protocol;

            AddSystemRuntimeType(null, typeof(void), new VoidTypeMap(), null);

            var qualifiedName = XName.Get("date", NamespaceConstants.XSD);
            ITypeMap arrayTypeMap = new ArrayTypeMap<DateTime>(this, qualifiedName);
            AddSystemXmlType(qualifiedName, typeof(DateTime), DateTypeMap.Instance, arrayTypeMap);
            AddSystemRuntimeType(qualifiedName, typeof(DateTime), DateTypeMap.Instance, arrayTypeMap);

            qualifiedName = XName.Get("dateTime", NamespaceConstants.XSD);
            arrayTypeMap = new ArrayTypeMap<DateTime>(this, qualifiedName);
            AddSystemXmlType(qualifiedName, typeof(DateTime), DateTimeTypeMap.Instance, arrayTypeMap);
            AddSystemRuntimeType(qualifiedName, typeof(DateTime), DateTimeTypeMap.Instance, arrayTypeMap);

            qualifiedName = XName.Get("boolean", NamespaceConstants.XSD);
            arrayTypeMap = new ArrayTypeMap<bool>(this, qualifiedName);
            AddSystemXmlType(qualifiedName, typeof(bool), BooleanTypeMap.Instance, arrayTypeMap);
            AddSystemRuntimeType(qualifiedName, typeof(bool), BooleanTypeMap.Instance, arrayTypeMap);

            qualifiedName = XName.Get("float", NamespaceConstants.XSD);
            arrayTypeMap = new ArrayTypeMap<float>(this, qualifiedName);
            AddSystemXmlType(qualifiedName, typeof(float), FloatTypeMap.Instance, arrayTypeMap);
            AddSystemRuntimeType(qualifiedName, typeof(float), FloatTypeMap.Instance, arrayTypeMap);

            qualifiedName = XName.Get("float", NamespaceConstants.XSD);
            arrayTypeMap = new ArrayTypeMap<double>(this, qualifiedName);
            AddSystemXmlType(qualifiedName, typeof(double), DoubleTypeMap.Instance, arrayTypeMap);
            AddSystemRuntimeType(qualifiedName, typeof(double), DoubleTypeMap.Instance, arrayTypeMap);

            qualifiedName = XName.Get("decimal", NamespaceConstants.XSD);
            arrayTypeMap = new ArrayTypeMap<decimal>(this, qualifiedName);
            AddSystemXmlType(qualifiedName, typeof(decimal), DecimalTypeMap.Instance, arrayTypeMap);
            AddSystemRuntimeType(qualifiedName, typeof(decimal), DecimalTypeMap.Instance, arrayTypeMap);

            qualifiedName = XName.Get("long", NamespaceConstants.XSD);
            arrayTypeMap = new ArrayTypeMap<long>(this, qualifiedName);
            AddSystemXmlType(qualifiedName, typeof(long), LongTypeMap.Instance, arrayTypeMap);
            AddSystemRuntimeType(qualifiedName, typeof(long), LongTypeMap.Instance, arrayTypeMap);

            qualifiedName = XName.Get("int", NamespaceConstants.XSD);
            arrayTypeMap = new ArrayTypeMap<int>(this, qualifiedName);
            AddSystemXmlType(qualifiedName, typeof(int), IntegerTypeMap.Instance, arrayTypeMap);
            AddSystemRuntimeType(qualifiedName, typeof(int), IntegerTypeMap.Instance, arrayTypeMap);
            AddSystemRuntimeType(qualifiedName, typeof(short), IntegerTypeMap.Instance, arrayTypeMap);

            qualifiedName = XName.Get("integer", NamespaceConstants.XSD);
            arrayTypeMap = new ArrayTypeMap<int>(this, qualifiedName);
            AddSystemXmlType(qualifiedName, typeof(int), new IntegerTypeMap(qualifiedName), arrayTypeMap);

            qualifiedName = XName.Get("string", NamespaceConstants.XSD);
            arrayTypeMap = new ArrayTypeMap<int>(this, qualifiedName);
            AddSystemXmlType(qualifiedName, typeof(string), StringTypeMap.Instance, arrayTypeMap);
            AddSystemRuntimeType(qualifiedName, typeof(string), StringTypeMap.Instance, arrayTypeMap);

            qualifiedName = XName.Get("anyURI", NamespaceConstants.XSD);
            arrayTypeMap = new ArrayTypeMap<int>(this, qualifiedName);
            AddSystemXmlType(qualifiedName, typeof(string), new StringTypeMap(qualifiedName), arrayTypeMap);

            qualifiedName = XName.Get("hexBinary", NamespaceConstants.XSD);
            ITypeMap typeMap = new StreamTypeMap(qualifiedName);
            arrayTypeMap = new ArrayTypeMap<int>(this, qualifiedName);
            AddSystemXmlType(qualifiedName, typeof(Stream), typeMap, arrayTypeMap);
            AddSystemRuntimeType(qualifiedName, typeof(Stream), typeMap, arrayTypeMap);

            qualifiedName = XName.Get("base64", NamespaceConstants.XSD);
            typeMap = new StreamTypeMap(qualifiedName);
            arrayTypeMap = new ArrayTypeMap<int>(this, qualifiedName);
            AddSystemXmlType(qualifiedName, typeof(Stream), typeMap, arrayTypeMap);

            qualifiedName = XName.Get("base64Binary", NamespaceConstants.XSD);
            typeMap = new StreamTypeMap(qualifiedName);
            arrayTypeMap = new ArrayTypeMap<int>(this, qualifiedName);
            AddSystemXmlType(qualifiedName, typeof(Stream), typeMap, arrayTypeMap);

            var legacyProtocol = protocol as ILegacyProtocol;
            if (legacyProtocol == null)
                return;

            CreateTestSystem(legacyProtocol);
            CreateGetState(legacyProtocol);
            CreateListMethods(legacyProtocol);
        }

        public IServiceMap GetServiceMap(string operationName)
        {
            return GetServiceMap(XName.Get(operationName, protocol.ProducerNamespace));
        }

        public IServiceMap GetServiceMap(XName qualifiedName)
        {
            if (qualifiedName == null)
                return null;

            IServiceMap serviceMap;
            if (!serviceMaps.TryGetValue(qualifiedName, out serviceMap))
                serviceMap = AddServiceMap(qualifiedName);

            return serviceMap;
        }

        private IServiceMap AddServiceMap(XName qualifiedName)
        {
            var methodInfo = GetServiceInterface(contractAssembly, qualifiedName);
            if (methodInfo == null)
                throw XRoadException.UnknownType(qualifiedName.ToString());

            var operationDefinition = MetaDataConverter.ConvertOperation(methodInfo, qualifiedName);
            protocol.ExportOperation(operationDefinition);

            var parameterMaps = methodInfo.GetParameters().Select(x => CreateParameterMap(operationDefinition, x)).ToList();
            var resultMap = CreateParameterMap(operationDefinition, methodInfo.ReturnParameter);

            return serviceMaps.GetOrAdd(qualifiedName, new ServiceMap(operationDefinition, parameterMaps, resultMap));
        }

        private static MethodInfo GetServiceInterface(Assembly typeAssembly, XName qualifiedName)
        {
            return typeAssembly?.GetTypes()
                                .Where(t => t.IsInterface)
                                .SelectMany(t => t.GetMethods())
                                .SingleOrDefault(x => x.GetServices(true).Any(m => m == qualifiedName.LocalName));
        }

        private IParameterMap CreateParameterMap(OperationDefinition operationDefinition, ParameterInfo parameterInfo)
        {
            var parameterDefinition = MetaDataConverter.ConvertParameter(parameterInfo, operationDefinition);
            protocol.ExportParameter(parameterDefinition);

            var typeMap = parameterDefinition.TypeDefinition.Name != null
                ? GetTypeMap(parameterDefinition.TypeDefinition.Name, parameterInfo.ParameterType.IsArray)
                : GetTypeMap(parameterInfo.ParameterType);

            return new ParameterMap(this, parameterDefinition, typeMap);
        }

        public ITypeMap GetTypeMapFromXsiType(XmlReader reader)
        {
            var typeValue = reader.GetTypeAttributeValue();
            return typeValue == null ? null : GetTypeMap(typeValue.Item1, typeValue.Item2);
        }

        public ITypeMap GetTypeMap(Type runtimeType, IDictionary<Type, ITypeMap> partialTypeMaps = null)
        {
            if (runtimeType == null)
                return null;

            ITypeMap typeMap;
            if (!runtimeTypeMaps.TryGetValue(runtimeType, out typeMap) && (partialTypeMaps == null || !partialTypeMaps.TryGetValue(runtimeType, out typeMap)))
                typeMap = AddTypeMap(runtimeType, partialTypeMaps);

            return typeMap;
        }

        public ITypeMap GetTypeMap(XName qualifiedName, bool isArray)
        {
            if (qualifiedName == null)
                return null;

            Tuple<ITypeMap, ITypeMap> typeMaps;
            if (!xmlTypeMaps.TryGetValue(qualifiedName, out typeMaps))
                typeMaps = AddTypeMap(qualifiedName);

            return isArray ? typeMaps?.Item2 : typeMaps?.Item1;
        }

        private ITypeMap AddTypeMap(Type runtimeType, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            var typeDefinition = MetaDataConverter.ConvertType(runtimeType, protocol);
            protocol.ExportType(typeDefinition);

            ITypeMap typeMap;

            if (typeDefinition.RuntimeInfo.IsArray)
            {
                var itemTypeMap = GetTypeMap(typeDefinition.RuntimeInfo.GetElementType(), partialTypeMaps);
                var typeMapType = typeof(ArrayTypeMap<>).MakeGenericType(itemTypeMap.RuntimeType);
                typeMap = (ITypeMap)Activator.CreateInstance(typeMapType, this);
            }
            else
            {
                if (typeDefinition.RuntimeInfo.Assembly != contractAssembly)
                    return null;

                if (!typeDefinition.RuntimeInfo.IsAbstract && typeDefinition.RuntimeInfo.GetConstructor(Type.EmptyTypes) == null)
                    throw XRoadException.NoDefaultConstructorForType(typeDefinition.RuntimeInfo.Name);

                if (typeDefinition.RuntimeInfo.IsAbstract)
                    typeMap = (ITypeMap)Activator.CreateInstance(typeof(AbstractTypeMap<>).MakeGenericType(typeDefinition.RuntimeInfo));
                else if (typeDefinition.HasStrictContentOrder)
                    typeMap = (ITypeMap)Activator.CreateInstance(typeof(SequenceTypeMap<>).MakeGenericType(typeDefinition.RuntimeInfo), this, typeDefinition);
                else
                    typeMap = (ITypeMap)Activator.CreateInstance(typeof(AllTypeMap<>).MakeGenericType(typeDefinition.RuntimeInfo), this, typeDefinition);
            }

            partialTypeMaps = partialTypeMaps ?? new Dictionary<Type, ITypeMap>();
            partialTypeMaps.Add(typeDefinition.RuntimeInfo, typeMap);
            typeMap.InitializeProperties(GetProperties(typeDefinition, partialTypeMaps));
            partialTypeMaps.Remove(typeDefinition.RuntimeInfo);

            return runtimeTypeMaps.GetOrAdd(runtimeType, typeMap);
        }

        private Tuple<ITypeMap, ITypeMap> AddTypeMap(XName qualifiedName)
        {
            var runtimeType = GetRuntimeType(qualifiedName);
            if (runtimeType == null)
                return null;

            var typeDefinition = MetaDataConverter.ConvertType(runtimeType, protocol);
            protocol.ExportType(typeDefinition);

            if (!typeDefinition.RuntimeInfo.IsAbstract && typeDefinition.RuntimeInfo.GetConstructor(Type.EmptyTypes) == null)
                throw XRoadException.NoDefaultConstructorForType(typeDefinition.Name);

            ITypeMap typeMap;
            if (typeDefinition.RuntimeInfo.IsAbstract)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(AbstractTypeMap<>).MakeGenericType(typeDefinition.RuntimeInfo));
            else if (typeDefinition.HasStrictContentOrder)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(SequenceTypeMap<>).MakeGenericType(typeDefinition.RuntimeInfo), this, typeDefinition);
            else
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(AllTypeMap<>).MakeGenericType(typeDefinition.RuntimeInfo), this, typeDefinition);

            var arrayTypeMap = (ITypeMap)Activator.CreateInstance(typeof(ArrayTypeMap<>).MakeGenericType(typeDefinition.RuntimeInfo), this);

            var partialTypeMaps = new Dictionary<Type, ITypeMap>
            {
                { typeMap.RuntimeType, typeMap },
                { arrayTypeMap.RuntimeType, arrayTypeMap }
            };

            typeMap.InitializeProperties(GetProperties(typeDefinition, partialTypeMaps));

            return xmlTypeMaps.GetOrAdd(qualifiedName, Tuple.Create(typeMap, arrayTypeMap));
        }

        private IEnumerable<PropertyDefinition> GetProperties(TypeDefinition typeDefinition, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            return typeDefinition.RuntimeInfo
                                 .GetAllPropertiesSorted(typeDefinition.ContentComparer, p => CreateProperty(p, typeDefinition, partialTypeMaps))
                                 .Where(d => d.State != DefinitionState.Ignored);
        }

        private PropertyDefinition CreateProperty(PropertyInfo propertyInfo, TypeDefinition typeDefinition, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            var propertyDefinition = MetaDataConverter.ConvertProperty(propertyInfo, typeDefinition, protocol, partialTypeMaps);
            protocol.ExportProperty(propertyDefinition);

            return propertyDefinition;
        }

        private Type GetRuntimeType(XName qualifiedName)
        {
            if (!qualifiedName.NamespaceName.StartsWith("http://", StringComparison.InvariantCulture))
            {
                var type = contractAssembly.GetType($"{qualifiedName.Namespace}.{qualifiedName.LocalName}");
                return type != null && type.IsXRoadSerializable() ? type : null;
            }

            if (!protocol.ProducerNamespace.Equals(qualifiedName.NamespaceName))
                throw XRoadException.TundmatuNimeruum(qualifiedName.NamespaceName);

            var runtimeType = contractAssembly.GetTypes()
                                              .Where(type => type.Name.Equals(qualifiedName.LocalName))
                                              .SingleOrDefault(type => type.IsXRoadSerializable());
            if (runtimeType != null)
                return runtimeType;

            throw XRoadException.UnknownType(qualifiedName.ToString());
        }
        
        public XName GetXmlTypeName(Type type)
        {
            if (type.IsNullable())
                return GetXmlTypeName(Nullable.GetUnderlyingType(type));

            switch (type.FullName)
            {
                case "System.Byte": return XName.Get("byte", NamespaceConstants.XSD);
                case "System.DateTime": return XName.Get("dateTime", NamespaceConstants.XSD);
                case "System.Boolean": return XName.Get("boolean", NamespaceConstants.XSD);
                case "System.Single": return XName.Get("float", NamespaceConstants.XSD);
                case "System.Double": return XName.Get("double", NamespaceConstants.XSD);
                case "System.Decimal": return XName.Get("decimal", NamespaceConstants.XSD);
                case "System.Int64": return XName.Get("long", NamespaceConstants.XSD);
                case "System.Int32": return XName.Get("int", NamespaceConstants.XSD);
                case "System.String": return XName.Get("string", NamespaceConstants.XSD);
            }

            if (type.Assembly == contractAssembly)
                return XName.Get(type.Name, protocol.ProducerNamespace);

            throw XRoadException.AndmetüübileVastavNimeruumPuudub(type.FullName);
        }

        private void CreateTestSystem(ILegacyProtocol legacyProtocol)
        {
            var operationDefinition = new OperationDefinition
            {
                Name = XName.Get("testSystem", legacyProtocol.XRoadNamespace),
                HasStrictContentOrder = true,
                State = DefinitionState.Hidden
            };
            protocol.ExportOperation(operationDefinition);

            serviceMaps.GetOrAdd(operationDefinition.Name, new ServiceMap(operationDefinition, null, null));
        }

        private void CreateGetState(ILegacyProtocol legacyProtocol)
        {
            var operationDefinition = new OperationDefinition
            {
                Name = XName.Get("getState", legacyProtocol.XRoadNamespace),
                HasStrictContentOrder = true,
                State = DefinitionState.Hidden
            };
            protocol.ExportOperation(operationDefinition);

            var resultParameter = new ParameterDefinition(operationDefinition);
            protocol.ExportParameter(resultParameter);

            var resultParameterMap = new ParameterMap(this, resultParameter, IntegerTypeMap.Instance);
            serviceMaps.GetOrAdd(operationDefinition.Name, new ServiceMap(operationDefinition, null, resultParameterMap));
        }

        private void CreateListMethods(ILegacyProtocol legacyProtocol)
        {
            var operationDefinition = new OperationDefinition
            {
                Name = XName.Get("listMethods", legacyProtocol.XRoadNamespace),
                HasStrictContentOrder = true,
                State = DefinitionState.Hidden
            };
            protocol.ExportOperation(operationDefinition);

            var resultParameter = new ParameterDefinition(operationDefinition);
            protocol.ExportParameter(resultParameter);

            var resultParameterMap = new ParameterMap(this, resultParameter, GetTypeMap(typeof(string[])));
            serviceMaps.GetOrAdd(operationDefinition.Name, new ServiceMap(operationDefinition, null, resultParameterMap));
        }

        private void AddSystemXmlType(XName qualifiedName, Type runtimeType, ITypeMap defaultTypeMap, ITypeMap defaultArrayTypeMap)
        {
            var typeDefinition = new TypeDefinition { Name = qualifiedName, RuntimeInfo = runtimeType };
            protocol.ExportType(typeDefinition);
            var typeMap = typeDefinition.TypeMap ?? defaultTypeMap;

            var arrayTypeDefinition = new TypeDefinition { RuntimeInfo = runtimeType.MakeArrayType() };
            protocol.ExportType(arrayTypeDefinition);
            var arrayTypeMap = arrayTypeDefinition.TypeMap ?? defaultArrayTypeMap;

            if (typeDefinition.Name != null && (typeMap != null || arrayTypeMap != null))
                xmlTypeMaps.TryAdd(typeDefinition.Name, Tuple.Create(typeMap, arrayTypeMap));
        }

        private void AddSystemRuntimeType(XName qualifiedName, Type runtimeType, ITypeMap defaultTypeMap, ITypeMap defaultArrayTypeMap)
        {
            var typeDefinition = new TypeDefinition { Name = qualifiedName, RuntimeInfo = runtimeType };
            protocol.ExportType(typeDefinition);
            var typeMap = typeDefinition.TypeMap ?? defaultTypeMap;

            var arrayTypeDefinition = new TypeDefinition { RuntimeInfo = runtimeType.MakeArrayType() };
            protocol.ExportType(arrayTypeDefinition);
            var arrayTypeMap = arrayTypeDefinition.TypeMap ?? defaultArrayTypeMap;

            if (typeDefinition.RuntimeInfo != null && typeMap != null)
                runtimeTypeMaps.TryAdd(typeDefinition.RuntimeInfo, typeMap);

            if (arrayTypeDefinition.RuntimeInfo != null && arrayTypeMap != null)
                runtimeTypeMaps.TryAdd(arrayTypeDefinition.RuntimeInfo, arrayTypeMap);
        }
    }
}