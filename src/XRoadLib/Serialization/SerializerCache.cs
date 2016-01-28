using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        private readonly ConcurrentDictionary<XName, ConcurrentDictionary<uint, IServiceMap>> serviceMaps = new ConcurrentDictionary<XName, ConcurrentDictionary<uint, IServiceMap>>();
        private readonly ConcurrentDictionary<XName, ConcurrentDictionary<uint, Tuple<ITypeMap, ITypeMap>>> xmlTypeMaps = new ConcurrentDictionary<XName, ConcurrentDictionary<uint, Tuple<ITypeMap, ITypeMap>>>();
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<uint, ITypeMap>> runtimeTypeMaps = new ConcurrentDictionary<Type, ConcurrentDictionary<uint, ITypeMap>>();
        private readonly ConcurrentDictionary<XName, Tuple<ITypeMap, ITypeMap>> systemXmlTypeMaps = new ConcurrentDictionary<XName, Tuple<ITypeMap, ITypeMap>>();
        private readonly ConcurrentDictionary<Type, ITypeMap> systemRuntimeTypeMaps = new ConcurrentDictionary<Type, ITypeMap>();

        public string ProducerNamespace => protocol.ProducerNamespace;

        public SerializerCache(Assembly contractAssembly, IProtocol protocol)
        {
            this.contractAssembly = contractAssembly;
            this.protocol = protocol;

            systemRuntimeTypeMaps.GetOrAdd(typeof(void), new VoidTypeMap());

            ITypeMap arrayTypeMap = new ArrayTypeMap<DateTime>(this);
            AddSystemXmlType("date", NamespaceConstants.XSD, DateTypeMap.Instance, arrayTypeMap);

            AddSystemXmlType("dateTime", NamespaceConstants.XSD, DateTimeTypeMap.Instance, arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(DateTime), DateTimeTypeMap.Instance);
            systemRuntimeTypeMaps.GetOrAdd(typeof(DateTime?), DateTimeTypeMap.Instance);
            systemRuntimeTypeMaps.GetOrAdd(typeof(DateTime[]), arrayTypeMap);

            arrayTypeMap = new ArrayTypeMap<bool>(this);
            AddSystemXmlType("boolean", NamespaceConstants.XSD, BooleanTypeMap.Instance, arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(bool), BooleanTypeMap.Instance);
            systemRuntimeTypeMaps.GetOrAdd(typeof(bool?), BooleanTypeMap.Instance);
            systemRuntimeTypeMaps.GetOrAdd(typeof(bool[]), arrayTypeMap);

            arrayTypeMap = new ArrayTypeMap<float>(this);
            AddSystemXmlType("float", NamespaceConstants.XSD, FloatTypeMap.Instance, arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(float), FloatTypeMap.Instance);
            systemRuntimeTypeMaps.GetOrAdd(typeof(float?), FloatTypeMap.Instance);
            systemRuntimeTypeMaps.GetOrAdd(typeof(float[]), arrayTypeMap);

            arrayTypeMap = new ArrayTypeMap<double>(this);
            AddSystemXmlType("double", NamespaceConstants.XSD, DoubleTypeMap.Instance, arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(double), DoubleTypeMap.Instance);
            systemRuntimeTypeMaps.GetOrAdd(typeof(double?), DoubleTypeMap.Instance);
            systemRuntimeTypeMaps.GetOrAdd(typeof(double[]), arrayTypeMap);

            arrayTypeMap = new ArrayTypeMap<decimal>(this);
            AddSystemXmlType("decimal", NamespaceConstants.XSD, DecimalTypeMap.Instance, arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(decimal), DecimalTypeMap.Instance);
            systemRuntimeTypeMaps.GetOrAdd(typeof(decimal?), DecimalTypeMap.Instance);
            systemRuntimeTypeMaps.GetOrAdd(typeof(decimal[]), arrayTypeMap);

            arrayTypeMap = new ArrayTypeMap<long>(this);
            AddSystemXmlType("long", NamespaceConstants.XSD, LongTypeMap.Instance, arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(long), LongTypeMap.Instance);
            systemRuntimeTypeMaps.GetOrAdd(typeof(long?), LongTypeMap.Instance);
            systemRuntimeTypeMaps.GetOrAdd(typeof(long[]), arrayTypeMap);

            arrayTypeMap = new ArrayTypeMap<int>(this);
            AddSystemXmlType("integer", NamespaceConstants.XSD, IntegerTypeMap.Instance, arrayTypeMap);
            AddSystemXmlType("int", NamespaceConstants.XSD, IntegerTypeMap.Instance, arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(int), IntegerTypeMap.Instance);
            systemRuntimeTypeMaps.GetOrAdd(typeof(int?), IntegerTypeMap.Instance);
            systemRuntimeTypeMaps.GetOrAdd(typeof(short), IntegerTypeMap.Instance);
            systemRuntimeTypeMaps.GetOrAdd(typeof(int[]), arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(short[]), arrayTypeMap);

            ITypeMap itemTypeMap = new StringTypeMap();
            arrayTypeMap = new ArrayTypeMap<string>(this);
            AddSystemXmlType("anyURI", NamespaceConstants.XSD, itemTypeMap, arrayTypeMap);
            AddSystemXmlType("string", NamespaceConstants.XSD, itemTypeMap, arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(string), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(string[]), arrayTypeMap);

            var qualifiedName = XName.Get("hexBinary", NamespaceConstants.SOAP_ENC);
            itemTypeMap = new StreamTypeMap(qualifiedName);
            systemXmlTypeMaps.GetOrAdd(qualifiedName, Tuple.Create(itemTypeMap, (ITypeMap)null));

            qualifiedName = XName.Get("base64", NamespaceConstants.SOAP_ENC);
            itemTypeMap = new StreamTypeMap(qualifiedName);
            systemXmlTypeMaps.GetOrAdd(qualifiedName, Tuple.Create(itemTypeMap, (ITypeMap)null));

            qualifiedName = XName.Get("base64Binary", NamespaceConstants.SOAP_ENC);
            itemTypeMap = new StreamTypeMap(qualifiedName);
            systemXmlTypeMaps.GetOrAdd(qualifiedName, Tuple.Create(itemTypeMap, (ITypeMap)null));

            if (protocol == XRoadProtocol.Version20)
            {
                systemRuntimeTypeMaps.GetOrAdd(typeof(System.IO.Stream), itemTypeMap);
                systemRuntimeTypeMaps.GetOrAdd(typeof(System.IO.FileStream), itemTypeMap);
                systemRuntimeTypeMaps.GetOrAdd(typeof(System.IO.MemoryStream), itemTypeMap);
            }

            qualifiedName = XName.Get("base64binary", NamespaceConstants.XSD);
            itemTypeMap = new StreamTypeMap(qualifiedName);
            systemXmlTypeMaps.GetOrAdd(qualifiedName, Tuple.Create(itemTypeMap, (ITypeMap)null));

            if (protocol != XRoadProtocol.Version20)
            {
                systemRuntimeTypeMaps.GetOrAdd(typeof(System.IO.Stream), itemTypeMap);
                systemRuntimeTypeMaps.GetOrAdd(typeof(System.IO.FileStream), itemTypeMap);
                systemRuntimeTypeMaps.GetOrAdd(typeof(System.IO.MemoryStream), itemTypeMap);
            }

            var legacyProtocol = protocol as ILegacyProtocol;
            if (legacyProtocol == null)
                return;

            CreateTestSystem(legacyProtocol);
            CreateGetState(legacyProtocol);
            CreateListMethods(legacyProtocol);
        }

        public IServiceMap GetServiceMap(string operationName, uint dtoVersion)
        {
            return GetServiceMap(XName.Get(operationName, ProducerNamespace), dtoVersion);
        }

        public IServiceMap GetServiceMap(XName qualifiedName, uint dtoVersion)
        {
            if (qualifiedName == null)
                return null;

            var serviceMapVersions = GetServiceMapVersions(qualifiedName);

            IServiceMap serviceMap;
            if (!serviceMapVersions.TryGetValue(dtoVersion, out serviceMap))
                serviceMap = AddServiceMap(serviceMapVersions, qualifiedName, dtoVersion);

            return serviceMap;
        }

        private ConcurrentDictionary<uint, IServiceMap> GetServiceMapVersions(XName qualifiedName)
        {
            ConcurrentDictionary<uint, IServiceMap> serviceMapVersions;
            return serviceMaps.TryGetValue(qualifiedName, out serviceMapVersions)
                ? serviceMapVersions
                : serviceMaps.GetOrAdd(qualifiedName, new ConcurrentDictionary<uint, IServiceMap>());
        }

        private IServiceMap AddServiceMap(ConcurrentDictionary<uint, IServiceMap> serviceMapVersions, XName qualifiedName, uint dtoVersion)
        {
            var methodInfo = GetServiceInterface(contractAssembly, qualifiedName, dtoVersion);
            if (methodInfo == null)
                throw XRoadException.UnknownType(qualifiedName.ToString());

            var operationDefinition = MetaDataConverter.ConvertOperation(methodInfo, qualifiedName);
            protocol.ExportOperation(operationDefinition);

            var parameterMaps = methodInfo.GetParameters()
                                          .Select(x => CreateParameterMap(operationDefinition, x, dtoVersion))
                                          .Where(m => m.ParameterInfo.ExistsInVersion(dtoVersion))
                                          .ToList();

            var resultMap = CreateParameterMap(operationDefinition, methodInfo.ReturnParameter, dtoVersion);

            return serviceMapVersions.GetOrAdd(dtoVersion, new ServiceMap(operationDefinition, parameterMaps, resultMap));
        }

        private static MethodInfo GetServiceInterface(Assembly typeAssembly, XName qualifiedName, uint dtoVersion)
        {
            return typeAssembly?.GetTypes()
                                .Where(t => t.IsInterface)
                                .SelectMany(t => t.GetMethods())
                                .SingleOrDefault(x => x.GetServicesInVersion(dtoVersion, true)
                                                       .Any(m => m == qualifiedName.LocalName));
        }

        private IParameterMap CreateParameterMap(OperationDefinition operationDefinition, ParameterInfo parameterInfo, uint dtoVersion)
        {
            var parameterDefinition = MetaDataConverter.ConvertParameter(parameterInfo, operationDefinition);
            protocol.ExportParameter(parameterDefinition);

            var typeMap = parameterDefinition.TypeDefinition.Name != null
                ? GetTypeMap(parameterDefinition.TypeDefinition.Name, parameterInfo.ParameterType.IsArray, dtoVersion)
                : GetTypeMap(parameterInfo.ParameterType, dtoVersion);

            return new ParameterMap(this, parameterDefinition, typeMap);
        }

        public ITypeMap GetTypeMapFromXsiType(XmlReader reader, uint dtoVersion)
        {
            var typeValue = reader.GetTypeAttributeValue();
            return typeValue == null ? null : GetTypeMap(typeValue.Item1, typeValue.Item2, dtoVersion);
        }

        public ITypeMap GetTypeMap(Type runtimeType, uint dtoVersion, IDictionary<Type, ITypeMap> partialTypeMaps = null)
        {
            if (runtimeType == null)
                return null;

            ITypeMap typeMap;

            if (runtimeType.FullName.StartsWith("System", StringComparison.InvariantCulture))
            {
                if (!systemRuntimeTypeMaps.TryGetValue(runtimeType, out typeMap))
                    throw XRoadException.UnknownType(runtimeType.FullName);
                return typeMap;
            }

            var typeMapVersions = GetTypeMapVersions(runtimeType);
            if (!typeMapVersions.TryGetValue(dtoVersion, out typeMap) && (partialTypeMaps == null || !partialTypeMaps.TryGetValue(runtimeType, out typeMap)))
                typeMap = AddTypeMap(typeMapVersions, runtimeType, dtoVersion, partialTypeMaps);

            return typeMap;
        }

        public ITypeMap GetTypeMap(XName qualifiedName, bool isArray, uint dtoVersion)
        {
            var typeMaps = GetTypeMaps(qualifiedName, dtoVersion);
            
            return isArray ? typeMaps?.Item2 : typeMaps?.Item1;
        }

        public Tuple<ITypeMap, ITypeMap> GetTypeMaps(XName qualifiedName, uint dtoVersion)
        {
            if (qualifiedName == null)
                return null;

            Tuple<ITypeMap, ITypeMap> typeMaps;

            if (NamespaceConstants.SOAP_ENC.Equals(qualifiedName.NamespaceName) || NamespaceConstants.XSD.Equals(qualifiedName.NamespaceName) || qualifiedName.NamespaceName.StartsWith("System", StringComparison.InvariantCulture))
            {
                if (!systemXmlTypeMaps.TryGetValue(qualifiedName, out typeMaps))
                    throw XRoadException.UnknownType(qualifiedName.ToString());
            }
            else
            {
                var typeMapVersions = GetTypeMapVersions(ref qualifiedName);
                if (!typeMapVersions.TryGetValue(dtoVersion, out typeMaps))
                    typeMaps = AddTypeMap(typeMapVersions, qualifiedName, dtoVersion);
            }

            return typeMaps;
        }

        private ConcurrentDictionary<uint, ITypeMap> GetTypeMapVersions(Type type)
        {
            ConcurrentDictionary<uint, ITypeMap> typeMapVersions;
            return runtimeTypeMaps.TryGetValue(type, out typeMapVersions) ? typeMapVersions : runtimeTypeMaps.GetOrAdd(type, new ConcurrentDictionary<uint, ITypeMap>());
        }

        private ConcurrentDictionary<uint, Tuple<ITypeMap, ITypeMap>> GetTypeMapVersions(ref XName qualifiedName)
        {
            ConcurrentDictionary<uint, Tuple<ITypeMap, ITypeMap>> typeMapVersions;
            if (xmlTypeMaps.TryGetValue(qualifiedName, out typeMapVersions))
                return typeMapVersions;

            var namespaceBase = protocol.GetProducerNamespaceBase(qualifiedName.NamespaceName);
            if (!string.IsNullOrWhiteSpace(namespaceBase))
            {
                qualifiedName = XName.Get(qualifiedName.LocalName, namespaceBase);
                if (xmlTypeMaps.TryGetValue(qualifiedName, out typeMapVersions))
                    return typeMapVersions;
            }

            return xmlTypeMaps.GetOrAdd(qualifiedName, new ConcurrentDictionary<uint, Tuple<ITypeMap, ITypeMap>>());
        }

        private ITypeMap AddTypeMap(ConcurrentDictionary<uint, ITypeMap> typeMapVersions, Type runtimeType, uint dtoVersion, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            var typeDefinition = MetaDataConverter.ConvertType(runtimeType, protocol);
            protocol.ExportType(typeDefinition);

            ITypeMap typeMap;

            if (typeDefinition.RuntimeInfo.IsArray)
            {
                var itemTypeMap = GetTypeMap(typeDefinition.RuntimeInfo.GetElementType(), dtoVersion, partialTypeMaps);
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

            typeMap.DtoVersion = dtoVersion;

            partialTypeMaps = partialTypeMaps ?? new Dictionary<Type, ITypeMap>();
            partialTypeMaps.Add(typeDefinition.RuntimeInfo, typeMap);
            typeMap.InitializeProperties(partialTypeMaps, GetProperties(typeDefinition, dtoVersion, partialTypeMaps));
            partialTypeMaps.Remove(typeDefinition.RuntimeInfo);

            return typeMapVersions.GetOrAdd(typeMap.DtoVersion, typeMap);
        }

        private Tuple<ITypeMap, ITypeMap> AddTypeMap(ConcurrentDictionary<uint, Tuple<ITypeMap, ITypeMap>> typeMapVersions, XName qualifiedName, uint dtoVersion)
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

            typeMap.DtoVersion = dtoVersion;
            arrayTypeMap.DtoVersion = dtoVersion;

            typeMap.InitializeProperties(partialTypeMaps, GetProperties(typeDefinition, dtoVersion, partialTypeMaps));

            return typeMapVersions.GetOrAdd(dtoVersion, Tuple.Create(typeMap, arrayTypeMap));
        }

        private IEnumerable<PropertyDefinition> GetProperties(TypeDefinition typeDefinition, uint dtoVersion, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            return typeDefinition.RuntimeInfo
                                 .GetAllPropertiesSorted(typeDefinition.ContentComparer, p => CreateProperty(p, typeDefinition, dtoVersion, partialTypeMaps))
                                 .Where(d => d.State != DefinitionState.Ignored);
        }

        private PropertyDefinition CreateProperty(PropertyInfo propertyInfo, TypeDefinition typeDefinition, uint dtoVersion, IDictionary<Type, ITypeMap> partialTypeMaps)
        {
            var propertyDefinition = MetaDataConverter.ConvertProperty(propertyInfo, typeDefinition, protocol, dtoVersion, partialTypeMaps);
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

            if (!ProducerNamespace.Equals(qualifiedName.NamespaceName))
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
                return XName.Get(type.Name, ProducerNamespace);

            throw XRoadException.AndmetüübileVastavNimeruumPuudub(type.FullName);
        }

        private void AddSystemXmlType(string typeName, string ns, ITypeMap itemTypeMap, ITypeMap arrayTypeMap)
        {
            systemXmlTypeMaps.GetOrAdd(XName.Get(typeName, ns), Tuple.Create(itemTypeMap, protocol == XRoadProtocol.Version20 ? arrayTypeMap : null));
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

            var serviceMap = new ConcurrentDictionary<uint, IServiceMap>();
            serviceMap.GetOrAdd(1u, new ServiceMap(operationDefinition, null, null));

            serviceMaps.GetOrAdd(operationDefinition.Name, serviceMap);
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

            var resultParameterMap = new ParameterMap(this, resultParameter, GetTypeMap(typeof(int), 1u));
            var serviceMap = new ConcurrentDictionary<uint, IServiceMap>();
            serviceMap.GetOrAdd(1u, new ServiceMap(operationDefinition, null, resultParameterMap));

            serviceMaps.GetOrAdd(operationDefinition.Name, serviceMap);
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

            var resultParameterMap = new ParameterMap(this, resultParameter, GetTypeMap(typeof(string[]), 1u));
            var serviceMap = new ConcurrentDictionary<uint, IServiceMap>();
            serviceMap.GetOrAdd(1u, new ServiceMap(operationDefinition, null, resultParameterMap));

            serviceMaps.GetOrAdd(operationDefinition.Name, serviceMap);
        }
    }
}