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
using XRoadLib.Schema;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Serialization
{
    public sealed class Serializer : ISerializer
    {
        private readonly Assembly _contractAssembly;
        private readonly SchemaDefinitionProvider _schemaDefinitionProvider;
        private readonly ICollection<string> _availableFilters;
        private readonly string _producerNamespace;

        private readonly ConcurrentDictionary<Type, ITypeMap> _customTypeMaps = new ConcurrentDictionary<Type, ITypeMap>();
        private readonly ConcurrentDictionary<XName, IServiceMap> _serviceMaps = new ConcurrentDictionary<XName, IServiceMap>();
        private readonly ConcurrentDictionary<XName, Tuple<ITypeMap, ITypeMap>> _xmlTypeMaps = new ConcurrentDictionary<XName, Tuple<ITypeMap, ITypeMap>>();
        private readonly ConcurrentDictionary<Type, ITypeMap> _runtimeTypeMaps = new ConcurrentDictionary<Type, ITypeMap>();

        public uint? Version { get; }

        public Serializer(SchemaDefinitionProvider schemaDefinitionProvider, uint? version = null)
        {
            _schemaDefinitionProvider = schemaDefinitionProvider;

            _availableFilters = schemaDefinitionProvider.ProtocolDefinition.EnabledFilters;
            _contractAssembly = schemaDefinitionProvider.ProtocolDefinition.ContractAssembly;
            _producerNamespace = schemaDefinitionProvider.ProtocolDefinition.ProducerNamespace;

            Version = version;

            AddSystemType<DateTime>("dateTime", x => new DateTimeTypeMap(x));
            AddSystemType<DateTime>("date", x => new DateTypeMap(x));

            AddSystemType<TimeSpan>("duration", x => new TimeSpanTypeMap(x));

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

            AddSystemType<Stream>("base64Binary", x => new ContentTypeMap(x));
            AddSystemType<Stream>("hexBinary", x => new ContentTypeMap(x));
            AddSystemType<Stream>("base64", x => new ContentTypeMap(x));

            AddSystemType<object>("", x => new AnyContentTypeMap(x, this));
        }

        public IServiceMap GetServiceMap(string operationName)
        {
            return GetServiceMap(XName.Get(operationName, _producerNamespace));
        }

        public IServiceMap GetServiceMap(XName qualifiedName)
        {
            if (qualifiedName == null)
                return null;

            return _serviceMaps.TryGetValue(qualifiedName, out var serviceMap)
                ? serviceMap
                : AddServiceMap(qualifiedName);
        }

        private IServiceMap AddServiceMap(XName qualifiedName)
        {
            var operationDefinition = GetOperationDefinition(_contractAssembly, qualifiedName);
            if (operationDefinition == null || qualifiedName.NamespaceName != operationDefinition.Name.NamespaceName)
                throw new UnknownOperationException(qualifiedName);

            var requestDefinition = _schemaDefinitionProvider.GetRequestDefinition(operationDefinition);
            var inputTypeMap = GetParticleDefinitionTypeMap(requestDefinition, null);

            var outputTuple = GetReturnValueTypeMap(operationDefinition);
            var responseDefinition = outputTuple.Item1;
            var outputTypeMap = outputTuple.Item2;

            var serviceMap = (IServiceMap)Activator.CreateInstance(
                operationDefinition.ServiceMapType,
                this,
                operationDefinition,
                requestDefinition,
                responseDefinition,
                inputTypeMap,
                outputTypeMap
            );

            return _serviceMaps.GetOrAdd(qualifiedName, serviceMap);
        }

        private OperationDefinition GetOperationDefinition(Assembly typeAssembly, XName qualifiedName)
        {
            return typeAssembly?.GetTypes()
                                .Where(t => t.GetTypeInfo().IsInterface)
                                .SelectMany(t => t.GetTypeInfo().GetMethods())
                                .Where(x => x.GetServices()
                                             .Any(m => m.Name == qualifiedName.LocalName
                                                       && (!Version.HasValue || m.ExistsInVersion(Version.Value))))
                                .Select(mi => _schemaDefinitionProvider.GetOperationDefinition(mi, qualifiedName, Version))
                                .SingleOrDefault(d => d.State != DefinitionState.Ignored);
        }

        public ITypeMap GetTypeMapFromXsiType(XmlReader reader, ParticleDefinition particleDefinition)
        {
            var qualifiedName = reader.GetTypeAttributeValue();
            return qualifiedName == null ? null : GetTypeMap(particleDefinition, qualifiedName);
        }

        public ITypeMap GetTypeMap(Type runtimeType, IDictionary<Type, ITypeMap> partialTypeMaps = null)
        {
            if (runtimeType == null)
                return null;

            var normalizedType = runtimeType.NormalizeType();

            return _runtimeTypeMaps.TryGetValue(normalizedType, out var typeMap) || (partialTypeMaps != null && partialTypeMaps.TryGetValue(normalizedType, out typeMap))
                ? typeMap
                : AddTypeMap(normalizedType, partialTypeMaps);
        }

        public ITypeMap GetTypeMap(ParticleDefinition particleDefinition, XName qualifiedName)
        {
            if (qualifiedName == null)
                return null;

            if (!_xmlTypeMaps.TryGetValue(qualifiedName, out var typeMaps))
                typeMaps = AddTypeMap(particleDefinition, qualifiedName);

            return particleDefinition.Content is ArrayContentDefiniton ? typeMaps?.Item2 : typeMaps?.Item1;
        }

        private ITypeMap AddTypeMap(Type runtimeType, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            if (runtimeType.IsXRoadSerializable() && Version.HasValue && !runtimeType.GetTypeInfo().ExistsInVersion(Version.Value))
                throw new SchemaDefinitionException($"The runtime type `{runtimeType.Name}` is not defined for DTO version `{Version.Value}`.");

            var typeDefinition = _schemaDefinitionProvider.GetTypeDefinition(runtimeType);

            ITypeMap typeMap;

            if (typeDefinition.TypeMapType != null)
            {
                typeMap = (ITypeMap)Activator.CreateInstance(typeDefinition.TypeMapType, this, typeDefinition);
                return _runtimeTypeMaps.GetOrAdd(runtimeType, typeMap);
            }

            if (typeDefinition is CollectionDefinition collectionDefinition)
            {
                var elementType = typeDefinition.Type.GetElementType();
                if (!ReferenceEquals(elementType.GetTypeInfo().Assembly, _contractAssembly))
                    return null;

                var itemTypeMap = GetTypeMap(elementType, partialTypeMaps);
                collectionDefinition.ItemDefinition = itemTypeMap.Definition;

                var typeMapType = typeof(ArrayTypeMap<>).MakeGenericType(itemTypeMap.Definition.Type);
                typeMap = (ITypeMap)Activator.CreateInstance(typeMapType, this, collectionDefinition, itemTypeMap);
                return _runtimeTypeMaps.GetOrAdd(runtimeType, typeMap);
            }

            if (!ReferenceEquals(typeDefinition.Type.GetTypeInfo().Assembly, _contractAssembly))
                return null;

            if (typeDefinition.IsCompositeType && typeDefinition.Type.GetTypeInfo().GetConstructor(Type.EmptyTypes) == null)
                throw new SchemaDefinitionException($"The runtime type '{typeDefinition.Type.Name}' does not have default constructor.");

            if (typeDefinition.Type.GetTypeInfo().IsEnum)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(EnumTypeMap), typeDefinition);
            else if (typeDefinition.Type.GetTypeInfo().IsAbstract)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(AbstractTypeMap), typeDefinition);
            else if (typeDefinition.HasStrictContentOrder)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(SequenceTypeMap<>).MakeGenericType(typeDefinition.Type), this, typeDefinition);
            else
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(AllTypeMap<>).MakeGenericType(typeDefinition.Type), this, typeDefinition);

            if (!(typeMap is ICompositeTypeMap compositeTypeMap))
                return _runtimeTypeMaps.GetOrAdd(runtimeType, typeMap);

            partialTypeMaps ??= new Dictionary<Type, ITypeMap>();
            partialTypeMaps.Add(typeDefinition.Type, compositeTypeMap);
            compositeTypeMap.InitializeProperties(GetRuntimeProperties(typeDefinition, partialTypeMaps), _availableFilters);
            partialTypeMaps.Remove(typeDefinition.Type);

            return _runtimeTypeMaps.GetOrAdd(runtimeType, compositeTypeMap);
        }

        private Tuple<ITypeMap, ITypeMap> AddTypeMap(ParticleDefinition particleDefinition, XName qualifiedName)
        {
            var typeDefinition = GetRuntimeTypeDefinition(particleDefinition, qualifiedName);
            if (typeDefinition == null)
                return null;

            if (qualifiedName.NamespaceName != typeDefinition.Name.NamespaceName)
                throw new UnknownTypeException(particleDefinition, typeDefinition, qualifiedName);

            if (typeDefinition.IsCompositeType && typeDefinition.Type.GetTypeInfo().GetConstructor(Type.EmptyTypes) == null)
                throw new SchemaDefinitionException($"The runtime type '{typeDefinition.Name}' does not define default constructor.");

            ITypeMap typeMap;

            if (typeDefinition.TypeMapType != null)
                typeMap = (ITypeMap)Activator.CreateInstance(typeDefinition.TypeMapType, this, typeDefinition);
            else if (typeDefinition.Type.GetTypeInfo().IsEnum)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(EnumTypeMap), typeDefinition);
            else if (typeDefinition.Type.GetTypeInfo().IsAbstract)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(AbstractTypeMap), typeDefinition);
            else if (typeDefinition.HasStrictContentOrder)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(SequenceTypeMap<>).MakeGenericType(typeDefinition.Type), this, typeDefinition);
            else
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(AllTypeMap<>).MakeGenericType(typeDefinition.Type), this, typeDefinition);

            var arrayTypeMap = (ITypeMap)Activator.CreateInstance(typeof(ArrayTypeMap<>).MakeGenericType(typeDefinition.Type), this, _schemaDefinitionProvider.GetCollectionDefinition(typeDefinition), typeMap);
            var typeMapTuple = Tuple.Create(typeMap, arrayTypeMap);

            if (!(typeMap is ICompositeTypeMap compositeTypeMap))
                return _xmlTypeMaps.GetOrAdd(qualifiedName, typeMapTuple);

            var partialTypeMaps = new Dictionary<Type, ITypeMap>
            {
                { compositeTypeMap.Definition.Type, compositeTypeMap },
                { arrayTypeMap.Definition.Type, arrayTypeMap }
            };

            compositeTypeMap.InitializeProperties(GetRuntimeProperties(typeDefinition, partialTypeMaps), _availableFilters);

            return _xmlTypeMaps.GetOrAdd(qualifiedName, typeMapTuple);
        }

        private TypeDefinition GetRuntimeTypeDefinition(ParticleDefinition particleDefinition, XName qualifiedName)
        {
            if (!qualifiedName.NamespaceName.StartsWith("http://"))
            {
                var type = _contractAssembly.GetType($"{qualifiedName.Namespace}.{qualifiedName.LocalName}");
                return type != null && type.IsXRoadSerializable() ? _schemaDefinitionProvider.GetTypeDefinition(type) : null;
            }

            var typeDefinition = _contractAssembly.GetTypes()
                                                 .Where(type => type.IsXRoadSerializable())
                    .Where(type => !Version.HasValue || type.GetTypeInfo().ExistsInVersion(Version.Value))
                    .Select(type => _schemaDefinitionProvider.GetTypeDefinition(type))
                    .SingleOrDefault(definition => definition.Name == qualifiedName);

            if (typeDefinition != null)
                return typeDefinition;

            throw new UnknownTypeException(particleDefinition, null, qualifiedName);
        }

        public XName GetXmlTypeName(Type type)
        {
            if (type.IsNullable())
                return GetXmlTypeName(Nullable.GetUnderlyingType(type));

            switch (type.FullName)
            {
                case "System.Byte": return XName.Get("byte", NamespaceConstants.Xsd);
                case "System.DateTime": return XName.Get("dateTime", NamespaceConstants.Xsd);
                case "System.Boolean": return XName.Get("boolean", NamespaceConstants.Xsd);
                case "System.Single": return XName.Get("float", NamespaceConstants.Xsd);
                case "System.Double": return XName.Get("double", NamespaceConstants.Xsd);
                case "System.Decimal": return XName.Get("decimal", NamespaceConstants.Xsd);
                case "System.Int64": return XName.Get("long", NamespaceConstants.Xsd);
                case "System.Int32": return XName.Get("int", NamespaceConstants.Xsd);
                case "System.String": return XName.Get("string", NamespaceConstants.Xsd);
                case "System.TimeSpan": return XName.Get("duration", NamespaceConstants.Xsd);
            }

            if (ReferenceEquals(type.GetTypeInfo().Assembly, _contractAssembly))
                return XName.Get(type.Name, _producerNamespace);

            throw new SchemaDefinitionException($"XML namespace of runtime type `{type.FullName}` is undefined.");
        }

        private void AddSystemType<T>(string typeName, Func<TypeDefinition, ITypeMap> createTypeMap)
        {
            var typeDefinition = _schemaDefinitionProvider.GetSimpleTypeDefinition<T>(typeName);

            var typeMap = GetCustomTypeMap(typeDefinition.TypeMapType) ?? createTypeMap(typeDefinition);

            if (typeDefinition.Type != null)
                _runtimeTypeMaps.TryAdd(typeDefinition.Type, typeMap);

            var collectionDefinition = _schemaDefinitionProvider.GetCollectionDefinition(typeDefinition);
            var arrayTypeMap = GetCustomTypeMap(collectionDefinition.TypeMapType) ?? new ArrayTypeMap<T>(this, collectionDefinition, typeMap);

            if (collectionDefinition.Type != null)
                _runtimeTypeMaps.TryAdd(collectionDefinition.Type, arrayTypeMap);

            if (typeDefinition.Name != null)
                _xmlTypeMaps.TryAdd(typeDefinition.Name, Tuple.Create(typeMap, arrayTypeMap));
        }

        private IEnumerable<Tuple<PropertyDefinition, ITypeMap>> GetRuntimeProperties(TypeDefinition typeDefinition, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            return typeDefinition.Type
                                 .GetAllPropertiesSorted(typeDefinition.ContentComparer, Version, p => _schemaDefinitionProvider.GetPropertyDefinition(p, typeDefinition))
                                 .Where(d => d.Content.State != DefinitionState.Ignored)
                                 .Select(p =>
                                 {
                                     var typeMap = GetParticleDefinitionTypeMap(p, partialTypeMaps);
                                     p.Content.TypeName = typeMap.Definition.Name;
                                     return Tuple.Create(p, typeMap);
                                 });
        }

        private ITypeMap GetParticleDefinitionTypeMap(ParticleDefinition particleDefinition, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            return particleDefinition.Content.TypeName == null
                ? GetTypeMap(particleDefinition.Content.RuntimeType, partialTypeMaps)
                : GetTypeMap(particleDefinition, particleDefinition.Content.TypeName);
        }

        private ITypeMap GetCustomTypeMap(Type typeMapType)
        {
            if (typeMapType == null)
                return null;

            if (_customTypeMaps.TryGetValue(typeMapType, out var typeMap))
                return typeMap;

            typeMap = (ITypeMap)Activator.CreateInstance(typeMapType, null, this);

            return _customTypeMaps.GetOrAdd(typeMapType, typeMap);
        }

        private Tuple<ResponseDefinition, ITypeMap> GetReturnValueTypeMap(OperationDefinition operationDefinition, XRoadFaultPresentation? xRoadFaultPresentation = null)
        {
            var returnDefinition = _schemaDefinitionProvider.GetResponseDefinition(operationDefinition, xRoadFaultPresentation);
            if (returnDefinition.Content.State == DefinitionState.Ignored)
                return null;

            var outputTypeMap = GetParticleDefinitionTypeMap(returnDefinition, null);
            if (outputTypeMap != null)
                returnDefinition.Content.TypeName = outputTypeMap.Definition.Name;

            return Tuple.Create(returnDefinition, outputTypeMap);
        }
    }
}