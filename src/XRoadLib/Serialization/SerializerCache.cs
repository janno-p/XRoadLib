using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
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

        private readonly ConcurrentDictionary<Type, ITypeMap> customTypeMaps = new ConcurrentDictionary<Type, ITypeMap>();
        private readonly ConcurrentDictionary<XName, IServiceMap> serviceMaps = new ConcurrentDictionary<XName, IServiceMap>();
        private readonly ConcurrentDictionary<XName, Tuple<ITypeMap, ITypeMap>> xmlTypeMaps = new ConcurrentDictionary<XName, Tuple<ITypeMap, ITypeMap>>();
        private readonly ConcurrentDictionary<Type, ITypeMap> runtimeTypeMaps = new ConcurrentDictionary<Type, ITypeMap>();

        public IProtocol Protocol { get; }
        public uint? Version { get; }

        public SerializerCache(IProtocol protocol, Assembly contractAssembly, uint? version = null)
        {
            this.contractAssembly = contractAssembly;

            Protocol = protocol;
            Version = version;

            AddSystemType<DateTime>("dateTime", x => new DateTimeTypeMap(x));
            AddSystemType<DateTime>("date", x => new DateTypeMap(x));

            AddSystemType<bool>("boolean", x => new BooleanTypeMap(x));

            AddSystemType<float>("float", x => new SingleTypeMap(x));
            AddSystemType<double>("double", x => new DoubleTypeMap(x));
            AddSystemType<decimal>("decimal", x => new DecimalTypeMap(x));

            AddSystemType<long>("long", x => new Int64TypeMap(x));
            AddSystemType<int>("int", x => new Int32TypeMap(x));
            AddSystemType<short>("short", x => new Int16TypeMap(x));
            AddSystemType<BigInteger>("integer", x => new IntegerTypeMap(x));

            AddSystemType<string>("string", x => new StringTypeMap(x));
            AddSystemType<string>("anyURI", x => new StringTypeMap(x));

            AddSystemType<Stream>("base64Binary", x => new StreamTypeMap(x));
            AddSystemType<Stream>("hexBinary", x => new StreamTypeMap(x));
            AddSystemType<Stream>("base64", x => new StreamTypeMap(x));

            var legacyProtocol = protocol as ILegacyProtocol;
            if (legacyProtocol == null)
                return;

            CreateTestSystem(legacyProtocol);
            CreateGetState(legacyProtocol);
            CreateListMethods(legacyProtocol);
        }

        public IServiceMap GetServiceMap(string operationName)
        {
            return GetServiceMap(XName.Get(operationName, Protocol.ProducerNamespace));
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

            var operationDefinition = Protocol.SchemaExporter.GetOperationDefinition(methodInfo, qualifiedName);

            var parameterMaps = operationDefinition.MethodInfo
                                                   .GetParameters()
                                                   .Where(p => !Version.HasValue || p.ExistsInVersion(Version.Value))
                                                   .Select(p => CreateParameterMap(operationDefinition, p))
                                                   .Where(p => p != null)
                                                   .ToList();

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
            var parameterDefinition = Protocol.SchemaExporter.GetParameterDefinition(parameterInfo, operationDefinition);
            if (parameterDefinition.State == DefinitionState.Ignored)
                return null;

            var typeMap = GetContentDefinitionTypeMap(parameterDefinition, null);
            parameterDefinition.TypeName = typeMap.Definition.Name;

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

            var normalizedType = Nullable.GetUnderlyingType(runtimeType) ?? runtimeType;

            ITypeMap typeMap;
            if (!runtimeTypeMaps.TryGetValue(normalizedType, out typeMap) && (partialTypeMaps == null || !partialTypeMaps.TryGetValue(normalizedType, out typeMap)))
                typeMap = AddTypeMap(normalizedType, partialTypeMaps);

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
            var typeDefinition = Protocol.SchemaExporter.GetTypeDefinition(runtimeType);

            ITypeMap typeMap;

            var collectionDefinition = typeDefinition as CollectionDefinition;
            if (collectionDefinition != null)
            {
                var itemTypeMap = GetTypeMap(typeDefinition.Type.GetElementType(), partialTypeMaps);
                collectionDefinition.ItemDefinition = itemTypeMap.Definition;

                var typeMapType = typeof(ArrayTypeMap<>).MakeGenericType(itemTypeMap.Definition.Type);
                typeMap = (ITypeMap)Activator.CreateInstance(typeMapType, this, collectionDefinition, itemTypeMap);
                return runtimeTypeMaps.GetOrAdd(runtimeType, typeMap);
            }

            if (typeDefinition.Type.Assembly != contractAssembly)
                return null;

            if (!typeDefinition.Type.IsAbstract && typeDefinition.Type.GetConstructor(Type.EmptyTypes) == null)
                throw XRoadException.NoDefaultConstructorForType(typeDefinition.Type.Name);

            if (typeDefinition.Type.IsAbstract)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(AbstractTypeMap<>).MakeGenericType(typeDefinition.Type), typeDefinition);
            else if (typeDefinition.HasStrictContentOrder)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(SequenceTypeMap<>).MakeGenericType(typeDefinition.Type), this, typeDefinition);
            else
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(AllTypeMap<>).MakeGenericType(typeDefinition.Type), this, typeDefinition);

            partialTypeMaps = partialTypeMaps ?? new Dictionary<Type, ITypeMap>();
            partialTypeMaps.Add(typeDefinition.Type, typeMap);
            typeMap.InitializeProperties(GetRuntimeProperties(typeDefinition, partialTypeMaps));
            partialTypeMaps.Remove(typeDefinition.Type);

            return runtimeTypeMaps.GetOrAdd(runtimeType, typeMap);
        }

        private Tuple<ITypeMap, ITypeMap> AddTypeMap(XName qualifiedName)
        {
            var runtimeType = GetRuntimeType(qualifiedName);
            if (runtimeType == null)
                return null;

            var typeDefinition = Protocol.SchemaExporter.GetTypeDefinition(runtimeType);

            if (!typeDefinition.Type.IsAbstract && typeDefinition.Type.GetConstructor(Type.EmptyTypes) == null)
                throw XRoadException.NoDefaultConstructorForType(typeDefinition.Name);

            ITypeMap typeMap;
            if (typeDefinition.Type.IsAbstract)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(AbstractTypeMap<>).MakeGenericType(typeDefinition.Type), typeDefinition);
            else if (typeDefinition.HasStrictContentOrder)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(SequenceTypeMap<>).MakeGenericType(typeDefinition.Type), this, typeDefinition);
            else
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(AllTypeMap<>).MakeGenericType(typeDefinition.Type), this, typeDefinition);

            var arrayTypeMap = (ITypeMap)Activator.CreateInstance(typeof(ArrayTypeMap<>).MakeGenericType(typeDefinition.Type), this, typeDefinition.CreateCollectionDefinition(), typeMap);

            var partialTypeMaps = new Dictionary<Type, ITypeMap>
            {
                { typeMap.Definition.Type, typeMap },
                { arrayTypeMap.Definition.Type, arrayTypeMap }
            };

            typeMap.InitializeProperties(GetRuntimeProperties(typeDefinition, partialTypeMaps));

            return xmlTypeMaps.GetOrAdd(qualifiedName, Tuple.Create(typeMap, arrayTypeMap));
        }

        private Type GetRuntimeType(XName qualifiedName)
        {
            if (!qualifiedName.NamespaceName.StartsWith("http://", StringComparison.InvariantCulture))
            {
                var type = contractAssembly.GetType($"{qualifiedName.Namespace}.{qualifiedName.LocalName}");
                return type != null && type.IsXRoadSerializable() ? type : null;
            }

            if (!Protocol.ProducerNamespace.Equals(qualifiedName.NamespaceName))
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
                return XName.Get(type.Name, Protocol.ProducerNamespace);

            throw XRoadException.AndmetüübileVastavNimeruumPuudub(type.FullName);
        }

        private void CreateTestSystem(ILegacyProtocol legacyProtocol)
        {
            var operationDefinition = Protocol.SchemaExporter.GetOperationDefinition(null, XName.Get("testSystem", legacyProtocol.XRoadNamespace));
            operationDefinition.State = DefinitionState.Hidden;

            serviceMaps.GetOrAdd(operationDefinition.Name, new ServiceMap(operationDefinition, null, null));
        }

        private void CreateGetState(ILegacyProtocol legacyProtocol)
        {
            var operationDefinition = Protocol.SchemaExporter.GetOperationDefinition(null, XName.Get("getState", legacyProtocol.XRoadNamespace));
            operationDefinition.State = DefinitionState.Hidden;

            var resultParameter = new ParameterDefinition(operationDefinition) { RuntimeType = typeof(int), IsResult = true };
            Protocol.SchemaExporter.ExportParameterDefinition(resultParameter);

            var typeMap = GetContentDefinitionTypeMap(resultParameter, null);
            resultParameter.TypeName = typeMap.Definition.Name;

            var resultParameterMap = new ParameterMap(this, resultParameter, typeMap);
            var serviceMap = new ServiceMap(operationDefinition, null, resultParameterMap);

            serviceMaps.GetOrAdd(operationDefinition.Name, serviceMap);
        }

        private void CreateListMethods(ILegacyProtocol legacyProtocol)
        {
            var operationDefinition = Protocol.SchemaExporter.GetOperationDefinition(null, XName.Get("listMethods", legacyProtocol.XRoadNamespace));
            operationDefinition.State = DefinitionState.Hidden;

            var resultParameter = new ParameterDefinition(operationDefinition) { RuntimeType = typeof(string[]), IsResult = true };
            Protocol.SchemaExporter.ExportParameterDefinition(resultParameter);

            var typeMap = GetContentDefinitionTypeMap(resultParameter, null);
            resultParameter.TypeName = typeMap.Definition.Name;

            var resultParameterMap = new ParameterMap(this, resultParameter, typeMap);
            serviceMaps.GetOrAdd(operationDefinition.Name, new ServiceMap(operationDefinition, null, resultParameterMap));
        }

        private void AddSystemType<T>(string typeName, Func<TypeDefinition, ITypeMap> createTypeMap)
        {
            var typeDefinition = TypeDefinition.SimpleTypeDefinition<T>(typeName);
            Protocol.SchemaExporter.ExportTypeDefinition(typeDefinition);

            var typeMap = GetCustomTypeMap(typeDefinition.TypeMapType) ?? createTypeMap(typeDefinition);

            if (typeDefinition.Type != null)
                runtimeTypeMaps.TryAdd(typeDefinition.Type, typeMap);

            var arrayDefinition = typeDefinition.CreateCollectionDefinition();
            Protocol.SchemaExporter.ExportTypeDefinition(arrayDefinition);

            var arrayTypeMap = GetCustomTypeMap(arrayDefinition.TypeMapType) ?? new ArrayTypeMap<T>(this, arrayDefinition, typeMap);

            if (arrayDefinition.Type != null)
                runtimeTypeMaps.TryAdd(arrayDefinition.Type, arrayTypeMap);

            if (typeDefinition.Name != null)
                xmlTypeMaps.TryAdd(typeDefinition.Name, Tuple.Create(typeMap, arrayTypeMap));
        }

        private IEnumerable<Tuple<PropertyDefinition, ITypeMap>> GetRuntimeProperties(TypeDefinition typeDefinition, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            return typeDefinition.Type
                                 .GetAllPropertiesSorted(typeDefinition.ContentComparer, Version, p => Protocol.SchemaExporter.GetPropertyDefinition(p, typeDefinition))
                                 .Where(d => d.State != DefinitionState.Ignored)
                                 .Select(p =>
                                 {
                                     var typeMap = GetContentDefinitionTypeMap(p, partialTypeMaps);
                                     p.TypeName = typeMap.Definition.Name;
                                     return Tuple.Create(p, typeMap);
                                 });
        }

        private ITypeMap GetContentDefinitionTypeMap(IContentDefinition contentDefinition, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            var runtimeType = contentDefinition.RuntimeType;

            return contentDefinition.TypeName == null
                ? GetTypeMap(runtimeType, partialTypeMaps)
                : GetTypeMap(contentDefinition.TypeName, runtimeType.IsArray);
        }

        private ITypeMap GetCustomTypeMap(Type typeMapType)
        {
            if (typeMapType == null)
                return null;

            ITypeMap typeMap;
            if (customTypeMaps.TryGetValue(typeMapType, out typeMap))
                return typeMap;

            typeMap = (ITypeMap)Activator.CreateInstance(typeMapType, null, this);

            return customTypeMaps.GetOrAdd(typeMapType, typeMap);
        }
    }
}