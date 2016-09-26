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
    public sealed class SerializerCache : ISerializerCache
    {
        private readonly Assembly contractAssembly;
        private readonly SchemaDefinitionReader schemaDefinitionReader;

        private readonly ConcurrentDictionary<Type, ITypeMap> customTypeMaps = new ConcurrentDictionary<Type, ITypeMap>();
        private readonly ConcurrentDictionary<XName, IServiceMap> serviceMaps = new ConcurrentDictionary<XName, IServiceMap>();
        private readonly ConcurrentDictionary<XName, Tuple<ITypeMap, ITypeMap>> xmlTypeMaps = new ConcurrentDictionary<XName, Tuple<ITypeMap, ITypeMap>>();
        private readonly ConcurrentDictionary<Type, ITypeMap> runtimeTypeMaps = new ConcurrentDictionary<Type, ITypeMap>();

        private ICollection<string> availableFilters;

        public XRoadProtocol Protocol { get; }
        public uint? Version { get; }

        public IEnumerable<string> AvailableFilters { get { return availableFilters; } set { availableFilters = value != null ? new List<string>(value) : null; } }

        public SerializerCache(XRoadProtocol protocol, SchemaDefinitionReader schemaDefinitionReader, Assembly contractAssembly, uint? version = null)
        {
            this.schemaDefinitionReader = schemaDefinitionReader;
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

            AddSystemType<Stream>("base64Binary", x => new ContentTypeMap(x));
            AddSystemType<Stream>("hexBinary", x => new ContentTypeMap(x));
            AddSystemType<Stream>("base64", x => new ContentTypeMap(x));
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
            var operationDefinition = GetOperationDefinition(contractAssembly, qualifiedName);
            if (operationDefinition == null || qualifiedName.NamespaceName != operationDefinition.Name.NamespaceName)
                throw XRoadException.UnknownOperation(qualifiedName);

            var requestValueDefinition = schemaDefinitionReader.GetRequestValueDefinition(operationDefinition);

            var inputTypeMap = GetTypeMap(requestValueDefinition.ParameterInfo?.ParameterType);
            var outputTuple = GetReturnValueTypeMap(operationDefinition);

            return serviceMaps.GetOrAdd(qualifiedName, new ServiceMap(this, operationDefinition, requestValueDefinition, outputTuple.Item1, inputTypeMap, outputTuple.Item2));
        }

        private OperationDefinition GetOperationDefinition(Assembly typeAssembly, XName qualifiedName)
        {
            return typeAssembly?.GetTypes()
                                .Where(t => t.GetTypeInfo().IsInterface)
                                .SelectMany(t => t.GetTypeInfo().GetMethods())
                                .Where(x => x.GetServices()
                                             .Any(m => m.Name == qualifiedName.LocalName
                                                       && (!Version.HasValue || m.ExistsInVersion(Version.Value))))
                                .Select(mi => schemaDefinitionReader.GetOperationDefinition(mi, qualifiedName, Version))
                                .SingleOrDefault(d => d.State != DefinitionState.Ignored);
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
            if (runtimeType.IsXRoadSerializable() && Version.HasValue && !runtimeType.GetTypeInfo().ExistsInVersion(Version.Value))
                throw XRoadException.UnknownType(runtimeType.ToString());

            var typeDefinition = schemaDefinitionReader.GetTypeDefinition(runtimeType);

            ITypeMap typeMap;

            if (typeDefinition.TypeMapType != null)
            {
                typeMap = (ITypeMap)Activator.CreateInstance(typeDefinition.TypeMapType, this, typeDefinition);
                return runtimeTypeMaps.GetOrAdd(runtimeType, typeMap);
            }

            var collectionDefinition = typeDefinition as CollectionDefinition;
            if (collectionDefinition != null)
            {
                var elementType = typeDefinition.Type.GetElementType();
                if (elementType.GetTypeInfo().Assembly != contractAssembly)
                    return null;

                var itemTypeMap = GetTypeMap(elementType, partialTypeMaps);
                collectionDefinition.ItemDefinition = itemTypeMap.Definition;

                var typeMapType = typeof(ArrayTypeMap<>).MakeGenericType(itemTypeMap.Definition.Type);
                typeMap = (ITypeMap)Activator.CreateInstance(typeMapType, this, collectionDefinition, itemTypeMap);
                return runtimeTypeMaps.GetOrAdd(runtimeType, typeMap);
            }

            if (typeDefinition.Type.GetTypeInfo().Assembly != contractAssembly)
                return null;

            if (!typeDefinition.Type.GetTypeInfo().IsEnum && !typeDefinition.Type.GetTypeInfo().IsAbstract && typeDefinition.Type.GetTypeInfo().GetConstructor(Type.EmptyTypes) == null)
                throw XRoadException.NoDefaultConstructorForType(typeDefinition.Type.Name);

            if (typeDefinition.Type.GetTypeInfo().IsEnum)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(EnumTypeMap), typeDefinition);
            else if (typeDefinition.Type.GetTypeInfo().IsAbstract)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(AbstractTypeMap), typeDefinition);
            else if (typeDefinition.HasStrictContentOrder)
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(SequenceTypeMap<>).MakeGenericType(typeDefinition.Type), this, typeDefinition);
            else
                typeMap = (ITypeMap)Activator.CreateInstance(typeof(AllTypeMap<>).MakeGenericType(typeDefinition.Type), this, typeDefinition);

            var compositeTypeMap = typeMap as ICompositeTypeMap;
            if (compositeTypeMap == null)
                return runtimeTypeMaps.GetOrAdd(runtimeType, typeMap);

            partialTypeMaps = partialTypeMaps ?? new Dictionary<Type, ITypeMap>();
            partialTypeMaps.Add(typeDefinition.Type, compositeTypeMap);
            compositeTypeMap.InitializeProperties(GetRuntimeProperties(typeDefinition, partialTypeMaps), availableFilters);
            partialTypeMaps.Remove(typeDefinition.Type);

            return runtimeTypeMaps.GetOrAdd(runtimeType, compositeTypeMap);
        }

        private Tuple<ITypeMap, ITypeMap> AddTypeMap(XName qualifiedName)
        {
            var typeDefinition = GetRuntimeTypeDefinition(qualifiedName);
            if (typeDefinition == null)
                return null;

            if (qualifiedName.NamespaceName != typeDefinition.Name.NamespaceName)
                throw XRoadException.UnknownType(qualifiedName.ToString());

            if (!typeDefinition.Type.GetTypeInfo().IsEnum && !typeDefinition.Type.GetTypeInfo().IsAbstract && typeDefinition.Type.GetTypeInfo().GetConstructor(Type.EmptyTypes) == null)
                throw XRoadException.NoDefaultConstructorForType(typeDefinition.Name);

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

            var arrayTypeMap = (ITypeMap)Activator.CreateInstance(typeof(ArrayTypeMap<>).MakeGenericType(typeDefinition.Type), this, schemaDefinitionReader.GetCollectionDefinition(typeDefinition), typeMap);
            var typeMapTuple = Tuple.Create(typeMap, arrayTypeMap);

            var compositeTypeMap = typeMap as ICompositeTypeMap;
            if (compositeTypeMap == null)
                return xmlTypeMaps.GetOrAdd(qualifiedName, typeMapTuple);

            var partialTypeMaps = new Dictionary<Type, ITypeMap>
            {
                { compositeTypeMap.Definition.Type, compositeTypeMap },
                { arrayTypeMap.Definition.Type, arrayTypeMap }
            };

            compositeTypeMap.InitializeProperties(GetRuntimeProperties(typeDefinition, partialTypeMaps), availableFilters);

            return xmlTypeMaps.GetOrAdd(qualifiedName, typeMapTuple);
        }

        private TypeDefinition GetRuntimeTypeDefinition(XName qualifiedName)
        {
            if (!qualifiedName.NamespaceName.StartsWith("http://"))
            {
                var type = contractAssembly.GetType($"{qualifiedName.Namespace}.{qualifiedName.LocalName}");
                return type != null && type.IsXRoadSerializable() ? schemaDefinitionReader.GetTypeDefinition(type) : null;
            }

            var typeDefinition = contractAssembly.GetTypes()
                                                 .Where(type => type.IsXRoadSerializable())
                                                 .Where(type => !Version.HasValue || type.GetTypeInfo().ExistsInVersion(Version.Value))
                                                 .Select(type => schemaDefinitionReader.GetTypeDefinition(type))
                                                 .SingleOrDefault(definition => definition.Name == qualifiedName);
            if (typeDefinition != null)
                return typeDefinition;

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

            if (type.GetTypeInfo().Assembly == contractAssembly)
                return XName.Get(type.Name, Protocol.ProducerNamespace);

            throw XRoadException.AndmetüübileVastavNimeruumPuudub(type.FullName);
        }

        private void AddSystemType<T>(string typeName, Func<TypeDefinition, ITypeMap> createTypeMap)
        {
            var typeDefinition = schemaDefinitionReader.GetSimpleTypeDefinition<T>(typeName);

            var typeMap = GetCustomTypeMap(typeDefinition.TypeMapType) ?? createTypeMap(typeDefinition);

            if (typeDefinition.Type != null)
                runtimeTypeMaps.TryAdd(typeDefinition.Type, typeMap);

            var collectionDefinition = schemaDefinitionReader.GetCollectionDefinition(typeDefinition);
            var arrayTypeMap = GetCustomTypeMap(collectionDefinition.TypeMapType) ?? new ArrayTypeMap<T>(this, collectionDefinition, typeMap);

            if (collectionDefinition.Type != null)
                runtimeTypeMaps.TryAdd(collectionDefinition.Type, arrayTypeMap);

            if (typeDefinition.Name != null)
                xmlTypeMaps.TryAdd(typeDefinition.Name, Tuple.Create(typeMap, arrayTypeMap));
        }

        private IEnumerable<Tuple<PropertyDefinition, ITypeMap>> GetRuntimeProperties(TypeDefinition typeDefinition, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            return typeDefinition.Type
                                 .GetAllPropertiesSorted(typeDefinition.ContentComparer, Version, p => schemaDefinitionReader.GetPropertyDefinition(p, typeDefinition))
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
            return contentDefinition.TypeName == null
                ? GetTypeMap(contentDefinition.RuntimeType, partialTypeMaps)
                : GetTypeMap(contentDefinition.TypeName, contentDefinition.RuntimeType.IsArray);
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

        private Tuple<ResponseValueDefinition, ITypeMap> GetReturnValueTypeMap(OperationDefinition operationDefinition, XRoadFaultPresentation? xRoadFaultPresentation = null)
        {
            var returnValueDefinition = schemaDefinitionReader.GetResponseValueDefinition(operationDefinition, xRoadFaultPresentation);
            if (returnValueDefinition.State == DefinitionState.Ignored)
                return null;

            var outputTypeMap = GetContentDefinitionTypeMap(returnValueDefinition, null);
            if (outputTypeMap != null)
                returnValueDefinition.TypeName = outputTypeMap.Definition.Name;

            return Tuple.Create(returnValueDefinition, outputTypeMap);
        }
    }
}