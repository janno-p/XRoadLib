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

            var operationDefinition = MetaDataConverter.ConvertOperation(methodInfo, qualifiedName, Protocol);

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
            var parameterDefinition = MetaDataConverter.ConvertParameter(parameterInfo, operationDefinition, this);

            var typeMap = parameterDefinition.TypeMap.TypeDefinition.Name != null
                ? GetTypeMap(parameterDefinition.TypeMap.TypeDefinition.Name, parameterInfo.ParameterType.IsArray)
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
            var typeDefinition = MetaDataConverter.ConvertType(runtimeType, Protocol);

            ITypeMap typeMap;

            var collectionDefinition = typeDefinition as CollectionDefinition;
            if (collectionDefinition != null)
            {
                var itemTypeMap = GetTypeMap(typeDefinition.RuntimeInfo.GetElementType(), partialTypeMaps);
                collectionDefinition.ItemDefinition = itemTypeMap.TypeDefinition;

                var typeMapType = typeof(ArrayTypeMap<>).MakeGenericType(itemTypeMap.TypeDefinition.RuntimeInfo);
                typeMap = (ITypeMap)Activator.CreateInstance(typeMapType, this, collectionDefinition);
                return runtimeTypeMaps.GetOrAdd(runtimeType, typeMap);
            }

            if (typeDefinition.RuntimeInfo.Assembly != contractAssembly)
                return null;

            if (!typeDefinition.RuntimeInfo.IsAbstract && typeDefinition.RuntimeInfo.GetConstructor(Type.EmptyTypes) == null)
                throw XRoadException.NoDefaultConstructorForType(typeDefinition.RuntimeInfo.Name);

            if (typeDefinition.RuntimeInfo.IsAbstract)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(AbstractTypeMap<>).MakeGenericType(typeDefinition.RuntimeInfo), typeDefinition);
            else if (typeDefinition.HasStrictContentOrder)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(SequenceTypeMap<>).MakeGenericType(typeDefinition.RuntimeInfo), this, typeDefinition);
            else
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(AllTypeMap<>).MakeGenericType(typeDefinition.RuntimeInfo), this, typeDefinition);

            partialTypeMaps = partialTypeMaps ?? new Dictionary<Type, ITypeMap>();
            partialTypeMaps.Add(typeDefinition.RuntimeInfo, typeMap);
            typeMap.InitializeProperties(MetaDataConverter.GetRuntimeProperties(this, typeDefinition, partialTypeMaps));
            partialTypeMaps.Remove(typeDefinition.RuntimeInfo);

            return runtimeTypeMaps.GetOrAdd(runtimeType, typeMap);
        }

        private Tuple<ITypeMap, ITypeMap> AddTypeMap(XName qualifiedName)
        {
            var runtimeType = GetRuntimeType(qualifiedName);
            if (runtimeType == null)
                return null;

            var typeDefinition = MetaDataConverter.ConvertType(runtimeType, Protocol);

            if (!typeDefinition.RuntimeInfo.IsAbstract && typeDefinition.RuntimeInfo.GetConstructor(Type.EmptyTypes) == null)
                throw XRoadException.NoDefaultConstructorForType(typeDefinition.Name);

            ITypeMap typeMap;
            if (typeDefinition.RuntimeInfo.IsAbstract)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(AbstractTypeMap<>).MakeGenericType(typeDefinition.RuntimeInfo), typeDefinition);
            else if (typeDefinition.HasStrictContentOrder)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(SequenceTypeMap<>).MakeGenericType(typeDefinition.RuntimeInfo), this, typeDefinition);
            else
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(AllTypeMap<>).MakeGenericType(typeDefinition.RuntimeInfo), this, typeDefinition);

            var arrayTypeMap = (ITypeMap)Activator.CreateInstance(typeof(ArrayTypeMap<>).MakeGenericType(typeDefinition.RuntimeInfo), this, typeDefinition.CreateCollectionDefinition());

            var partialTypeMaps = new Dictionary<Type, ITypeMap>
            {
                { typeMap.TypeDefinition.RuntimeInfo, typeMap },
                { arrayTypeMap.TypeDefinition.RuntimeInfo, arrayTypeMap }
            };

            typeMap.InitializeProperties(MetaDataConverter.GetRuntimeProperties(this, typeDefinition, partialTypeMaps));

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
            var operationDefinition = MetaDataConverter.ConvertOperation(null, XName.Get("testSystem", legacyProtocol.XRoadNamespace), Protocol);
            operationDefinition.State = DefinitionState.Hidden;

            serviceMaps.GetOrAdd(operationDefinition.Name, new ServiceMap(operationDefinition, null, null));
        }

        private void CreateGetState(ILegacyProtocol legacyProtocol)
        {
            var operationDefinition = MetaDataConverter.ConvertOperation(null, XName.Get("getState", legacyProtocol.XRoadNamespace), Protocol);
            operationDefinition.State = DefinitionState.Hidden;

            var resultParameter = new ParameterDefinition(operationDefinition);
            Protocol.ExportParameter(resultParameter);

            var resultParameterMap = new ParameterMap(this, resultParameter, GetTypeMap(typeof(int)));
            serviceMaps.GetOrAdd(operationDefinition.Name, new ServiceMap(operationDefinition, null, resultParameterMap));
        }

        private void CreateListMethods(ILegacyProtocol legacyProtocol)
        {
            var operationDefinition = MetaDataConverter.ConvertOperation(null, XName.Get("listMethods", legacyProtocol.XRoadNamespace), Protocol);
            operationDefinition.State = DefinitionState.Hidden;

            var resultParameter = new ParameterDefinition(operationDefinition);
            Protocol.ExportParameter(resultParameter);

            var resultParameterMap = new ParameterMap(this, resultParameter, GetTypeMap(typeof(string[])));
            serviceMaps.GetOrAdd(operationDefinition.Name, new ServiceMap(operationDefinition, null, resultParameterMap));
        }

        private void AddSystemType<T>(string typeName, Func<TypeDefinition, ITypeMap> createTypeMap)
        {
            var typeDefinition = TypeDefinition.SimpleTypeDefinition<T>(typeName);
            Protocol.ExportType(typeDefinition);

            var typeMap = typeDefinition.TypeMap ?? createTypeMap(typeDefinition);
            typeMap.TypeDefinition.TypeMap = typeMap;

            if (typeDefinition.RuntimeInfo != null)
                runtimeTypeMaps.TryAdd(typeDefinition.RuntimeInfo, typeMap);

            var arrayDefinition = typeDefinition.CreateCollectionDefinition();
            Protocol.ExportType(arrayDefinition);

            var arrayTypeMap = arrayDefinition.TypeMap ?? new ArrayTypeMap<T>(this, arrayDefinition);
            arrayTypeMap.TypeDefinition.TypeMap = arrayTypeMap;

            if (arrayDefinition.RuntimeInfo != null)
                runtimeTypeMaps.TryAdd(arrayDefinition.RuntimeInfo, arrayTypeMap);

            if (typeDefinition.Name != null)
                xmlTypeMaps.TryAdd(typeDefinition.Name, Tuple.Create(typeMap, arrayTypeMap));
        }
    }
}