using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Attributes;
using XRoadLib.Extensions;
using XRoadLib.Protocols;
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

            ITypeMap itemTypeMap = new DateTypeMap();
            ITypeMap arrayTypeMap = new ArrayTypeMap<DateTime>(this);
            AddSystemXmlType("date", NamespaceConstants.XSD, itemTypeMap, arrayTypeMap);

            itemTypeMap = new DateTimeTypeMap();
            AddSystemXmlType("dateTime", NamespaceConstants.XSD, itemTypeMap, arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(DateTime), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(DateTime?), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(DateTime[]), arrayTypeMap);

            itemTypeMap = new BooleanTypeMap();
            arrayTypeMap = new ArrayTypeMap<bool>(this);
            AddSystemXmlType("boolean", NamespaceConstants.XSD, itemTypeMap, arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(bool), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(bool?), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(bool[]), arrayTypeMap);

            itemTypeMap = new FloatTypeMap();
            arrayTypeMap = new ArrayTypeMap<float>(this);
            AddSystemXmlType("float", NamespaceConstants.XSD, itemTypeMap, arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(float), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(float?), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(float[]), arrayTypeMap);

            itemTypeMap = new DoubleTypeMap();
            arrayTypeMap = new ArrayTypeMap<double>(this);
            AddSystemXmlType("double", NamespaceConstants.XSD, itemTypeMap, arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(double), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(double?), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(double[]), arrayTypeMap);

            itemTypeMap = new DecimalTypeMap();
            arrayTypeMap = new ArrayTypeMap<decimal>(this);
            AddSystemXmlType("decimal", NamespaceConstants.XSD, itemTypeMap, arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(decimal), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(decimal?), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(decimal[]), arrayTypeMap);

            itemTypeMap = new LongTypeMap();
            arrayTypeMap = new ArrayTypeMap<long>(this);
            AddSystemXmlType("long", NamespaceConstants.XSD, itemTypeMap, arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(long), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(long?), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(long[]), arrayTypeMap);

            itemTypeMap = new IntegerTypeMap();
            arrayTypeMap = new ArrayTypeMap<int>(this);
            AddSystemXmlType("integer", NamespaceConstants.XSD, itemTypeMap, arrayTypeMap);
            AddSystemXmlType("int", NamespaceConstants.XSD, itemTypeMap, arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(int), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(int?), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(short), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(int[]), arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(short[]), arrayTypeMap);

            var integerTypeMap = itemTypeMap;

            itemTypeMap = new StringTypeMap();
            arrayTypeMap = new ArrayTypeMap<string>(this);
            AddSystemXmlType("anyURI", NamespaceConstants.XSD, itemTypeMap, arrayTypeMap);
            AddSystemXmlType("string", NamespaceConstants.XSD, itemTypeMap, arrayTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(string), itemTypeMap);
            systemRuntimeTypeMaps.GetOrAdd(typeof(string[]), arrayTypeMap);

            var stringArrayTypeMap = arrayTypeMap;

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

            var legacyProtocol = protocol as LegacyProtocol;
            if (legacyProtocol == null)
                return;

            qualifiedName = XName.Get("testSystem", legacyProtocol.XRoadNamespace);
            var serviceMap = new ConcurrentDictionary<uint, IServiceMap>();
            serviceMap.GetOrAdd(1u, new ServiceMap(qualifiedName, null, null, null, XRoadContentLayoutMode.Strict, false, false));
            serviceMaps.GetOrAdd(qualifiedName, serviceMap);

            qualifiedName = XName.Get("getState", legacyProtocol.XRoadNamespace);
            serviceMap = new ConcurrentDictionary<uint, IServiceMap>();
            serviceMap.GetOrAdd(1u, new ServiceMap(qualifiedName, null, null, new ParameterMap(this, null, null, integerTypeMap, true), XRoadContentLayoutMode.Strict, false, false));
            serviceMaps.GetOrAdd(qualifiedName, serviceMap);

            qualifiedName = XName.Get("listMethods", legacyProtocol.XRoadNamespace);
            serviceMap = new ConcurrentDictionary<uint, IServiceMap>();
            serviceMap.GetOrAdd(1u, new ServiceMap(qualifiedName, null, null, new ParameterMap(this, null, null, stringArrayTypeMap, true), XRoadContentLayoutMode.Strict, false, false));
            serviceMaps.GetOrAdd(qualifiedName, serviceMap);
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

            var configuration = protocol.GetContractConfiguration(contractAssembly);
            var parameterMaps = methodInfo.GetParameters()
                                          .Select(x => CreateParameterMap(configuration.OperationConfiguration, x, dtoVersion))
                                          .Where(m => m.ParameterInfo.ExistsInVersion(dtoVersion))
                                          .ToList();

            var resultMap = CreateParameterMap(configuration.OperationConfiguration, methodInfo.ReturnParameter, dtoVersion);

            var multipartAttribute = methodInfo.GetSingleAttribute<XRoadAttachmentAttribute>();
            var hasMultipartRequest = multipartAttribute != null && multipartAttribute.HasMultipartRequest;
            var hasMultipartResponse = multipartAttribute != null && multipartAttribute.HasMultipartResponse;

            var layoutMode = (configuration.OperationConfiguration?.GetParameterLayout(methodInfo)).GetValueOrDefault();
            var serviceMap = new ServiceMap(qualifiedName, methodInfo, parameterMaps, resultMap, layoutMode, hasMultipartRequest, hasMultipartResponse);

            return serviceMapVersions.GetOrAdd(dtoVersion, serviceMap);
        }

        private static MethodInfo GetServiceInterface(Assembly typeAssembly, XName qualifiedName, uint dtoVersion)
        {
            return typeAssembly?.GetTypes()
                                .Where(t => t.IsInterface)
                                .SelectMany(t => t.GetMethods())
                                .SingleOrDefault(x => x.GetServicesInVersion(dtoVersion, true)
                                                       .Any(m => m == qualifiedName.LocalName));
        }

        private IParameterMap CreateParameterMap(IOperationConfiguration operationConfiguration, ParameterInfo parameterInfo, uint dtoVersion)
        {
            var parameterName = parameterInfo.GetParameterName(operationConfiguration);

            var qualifiedTypeName = parameterInfo.GetQualifiedElementDataType();
            var typeMap = qualifiedTypeName != null ? GetTypeMap(qualifiedTypeName, parameterInfo.ParameterType.IsArray, dtoVersion) : GetTypeMap(parameterInfo.ParameterType, dtoVersion);

            return new ParameterMap(this, parameterName, parameterInfo, typeMap, parameterInfo.IsRequiredElement());
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

            if (runtimeType.FullName.StartsWith("System"))
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
            if (qualifiedName == null)
                return null;

            Tuple<ITypeMap, ITypeMap> typeMap;

            if (NamespaceConstants.SOAP_ENC.Equals(qualifiedName.NamespaceName) || NamespaceConstants.XSD.Equals(qualifiedName.NamespaceName) || qualifiedName.NamespaceName.StartsWith("System"))
            {
                if (!systemXmlTypeMaps.TryGetValue(qualifiedName, out typeMap))
                    throw XRoadException.UnknownType(qualifiedName.ToString());
            }
            else
            {
                var typeMapVersions = GetTypeMapVersions(ref qualifiedName);
                if (!typeMapVersions.TryGetValue(dtoVersion, out typeMap))
                    typeMap = AddTypeMap(typeMapVersions, qualifiedName, dtoVersion);
            }

            return isArray ? typeMap.Item2 : typeMap.Item1;
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
            ITypeMap typeMap;

            if (runtimeType.IsArray)
            {
                var itemTypeMap = GetTypeMap(runtimeType.GetElementType(), dtoVersion, partialTypeMaps);
                var typeMapType = typeof(ArrayTypeMap<>).MakeGenericType(itemTypeMap.RuntimeType);
                typeMap = (ITypeMap)Activator.CreateInstance(typeMapType, this);
            }
            else
            {
                if (runtimeType.Assembly != contractAssembly)
                    return null;

                if (!runtimeType.IsAbstract && runtimeType.GetConstructor(Type.EmptyTypes) == null)
                    throw XRoadException.NoDefaultConstructorForType(runtimeType.Name);

                if (runtimeType.IsAbstract)
                    typeMap = (ITypeMap)Activator.CreateInstance(typeof(AbstractTypeMap<>).MakeGenericType(runtimeType));
                else
                {
                    var contentLayoutMode = (typeConfiguration?.GetContentLayoutMode(runtimeType)).GetValueOrDefault();
                    if (contentLayoutMode == XRoadContentLayoutMode.Flexible)
                        typeMap = (ITypeMap)Activator.CreateInstance(typeof(AllTypeMap<>).MakeGenericType(runtimeType), this, runtimeType.GetProducerTypeName(typeConfiguration, protocol));
                    else
                        typeMap = (ITypeMap)Activator.CreateInstance(typeof(SequenceTypeMap<>).MakeGenericType(runtimeType), this, runtimeType.GetProducerTypeName(typeConfiguration, protocol));
                }

                typeMap.IsAnonymous = runtimeType.IsAnonymous();
            }

            typeMap.DtoVersion = dtoVersion;

            partialTypeMaps = partialTypeMaps ?? new Dictionary<Type, ITypeMap>();
            partialTypeMaps.Add(runtimeType, typeMap);
            typeMap.InitializeProperties(partialTypeMaps, typeConfiguration);
            partialTypeMaps.Remove(runtimeType);

            return typeMapVersions.GetOrAdd(typeMap.DtoVersion, typeMap);
        }

        private Tuple<ITypeMap, ITypeMap> AddTypeMap(ConcurrentDictionary<uint, Tuple<ITypeMap, ITypeMap>> typeMapVersions, XName qualifiedName, uint dtoVersion)
        {
            var runtimeType = GetRuntimeType(qualifiedName);
            if (runtimeType == null)
                return null;

            if (!runtimeType.IsAbstract && runtimeType.GetConstructor(Type.EmptyTypes) == null)
                throw XRoadException.NoDefaultConstructorForType(runtimeType.Name);

            ITypeMap itemTypeMap;

            if (runtimeType.IsAbstract)
                itemTypeMap = (ITypeMap)Activator.CreateInstance(typeof(AbstractTypeMap<>).MakeGenericType(runtimeType));
            else
            {
                var contentLayoutMode = (typeConfiguration?.GetContentLayoutMode(runtimeType)).GetValueOrDefault();
                if (contentLayoutMode == XRoadContentLayoutMode.Flexible)
                    itemTypeMap = (ITypeMap)Activator.CreateInstance(typeof(AllTypeMap<>).MakeGenericType(runtimeType), this, runtimeType.GetProducerTypeName(typeConfiguration, protocol));
                else
                    itemTypeMap = (ITypeMap)Activator.CreateInstance(typeof(SequenceTypeMap<>).MakeGenericType(runtimeType), this, runtimeType.GetProducerTypeName(typeConfiguration, protocol));
            }

            var arrayTypeMap = (ITypeMap)Activator.CreateInstance(typeof(ArrayTypeMap<>).MakeGenericType(itemTypeMap.RuntimeType), this);

            var partialTypeMaps = new Dictionary<Type, ITypeMap>
            {
                { itemTypeMap.RuntimeType, itemTypeMap },
                { arrayTypeMap.RuntimeType, arrayTypeMap }
            };

            itemTypeMap.DtoVersion = dtoVersion;
            itemTypeMap.IsAnonymous = runtimeType.IsAnonymous();

            arrayTypeMap.DtoVersion = dtoVersion;

            itemTypeMap.InitializeProperties(partialTypeMaps, typeConfiguration);
            arrayTypeMap.InitializeProperties(partialTypeMaps, typeConfiguration);

            return typeMapVersions.GetOrAdd(dtoVersion, Tuple.Create(itemTypeMap, arrayTypeMap));
        }

        private Type GetRuntimeType(XName qualifiedName)
        {
            if (!qualifiedName.NamespaceName.StartsWith("http://"))
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
    }
}