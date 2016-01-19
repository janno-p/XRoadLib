using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Attributes;
using XRoadLib.Configuration;
using XRoadLib.Extensions;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Serialization
{
    public sealed class SerializerCache : ISerializerCache
    {
        private readonly ITypeConfigurationProvider typeConfigurationProvider;

        private readonly Assembly contractAssembly;
        private readonly XRoadProtocol protocol;

        private readonly ConcurrentDictionary<XName, ConcurrentDictionary<uint, ITypeMap>> typeMaps = new ConcurrentDictionary<XName, ConcurrentDictionary<uint, ITypeMap>>();
        private readonly ConcurrentDictionary<XName, ConcurrentDictionary<uint, IServiceMap>> serviceMaps = new ConcurrentDictionary<XName, ConcurrentDictionary<uint, IServiceMap>>();
        private readonly ConcurrentDictionary<XName, ITypeMap> systemTypeMaps = new ConcurrentDictionary<XName, ITypeMap>();

        public string ProducerNamespace { get; }

        public SerializerCache(Assembly contractAssembly, XRoadProtocol protocol)
        {
            this.contractAssembly = contractAssembly;
            this.protocol = protocol;

            var producerName = contractAssembly.GetProducerName();
            ProducerNamespace = protocol.GetProducerNamespace(producerName);

            typeConfigurationProvider = protocol.GetTypeConfiguration(contractAssembly);

            ITypeMap typeMap = new DateTypeMap();
            systemTypeMaps.GetOrAdd(XName.Get("date", NamespaceConstants.XSD), typeMap);

            typeMap = new DateTimeTypeMap();
            systemTypeMaps.GetOrAdd(XName.Get("dateTime", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(DateTime).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(DateTime?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<DateTime>(this);
            if (protocol == XRoadProtocol.Version20)
            {
                systemTypeMaps.GetOrAdd(XName.Get("date[]", NamespaceConstants.XSD), typeMap);
                systemTypeMaps.GetOrAdd(XName.Get("dateTime[]", NamespaceConstants.XSD), typeMap);
            }
            systemTypeMaps.GetOrAdd(typeof(DateTime[]).ToQualifiedName(), typeMap);

            typeMap = new BooleanTypeMap();
            systemTypeMaps.GetOrAdd(XName.Get("boolean", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(bool).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(bool?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<bool>(this);
            if (protocol == XRoadProtocol.Version20)
                systemTypeMaps.GetOrAdd(XName.Get("boolean[]", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(bool[]).ToQualifiedName(), typeMap);

            typeMap = new FloatTypeMap();
            systemTypeMaps.GetOrAdd(XName.Get("float", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(float).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(float?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<float>(this);
            if (protocol == XRoadProtocol.Version20)
                systemTypeMaps.GetOrAdd(XName.Get("float[]", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(float[]).ToQualifiedName(), typeMap);

            typeMap = new DoubleTypeMap();
            systemTypeMaps.GetOrAdd(XName.Get("double", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(double).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(double?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<double>(this);
            if (protocol == XRoadProtocol.Version20)
                systemTypeMaps.GetOrAdd(XName.Get("double[]", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(double[]).ToQualifiedName(), typeMap);

            typeMap = new DecimalTypeMap();
            systemTypeMaps.GetOrAdd(XName.Get("decimal", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(decimal).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(decimal?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<decimal>(this);
            if (protocol == XRoadProtocol.Version20)
                systemTypeMaps.GetOrAdd(XName.Get("decimal[]", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(decimal[]).ToQualifiedName(), typeMap);

            typeMap = new LongTypeMap();
            systemTypeMaps.GetOrAdd(XName.Get("long", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(long).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(long?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<long>(this);
            if (protocol == XRoadProtocol.Version20)
                systemTypeMaps.GetOrAdd(XName.Get("long[]", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(long[]).ToQualifiedName(), typeMap);

            typeMap = new IntegerTypeMap();
            var integerTypeMap = typeMap;
            systemTypeMaps.GetOrAdd(XName.Get("integer", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(XName.Get("int", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(int).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(int?).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(short).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<int>(this);
            if (protocol == XRoadProtocol.Version20)
            {
                systemTypeMaps.GetOrAdd(XName.Get("integer[]", NamespaceConstants.XSD), typeMap);
                systemTypeMaps.GetOrAdd(XName.Get("int[]", NamespaceConstants.XSD), typeMap);
            }
            systemTypeMaps.GetOrAdd(typeof(int[]).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(short[]).ToQualifiedName(), typeMap);

            typeMap = new StringTypeMap();
            systemTypeMaps.GetOrAdd(XName.Get("anyURI", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(XName.Get("string", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(string).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<string>(this);
            var stringArrayTypeMap = typeMap;
            if (protocol == XRoadProtocol.Version20)
                systemTypeMaps.GetOrAdd(XName.Get("string[]", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(string[]).ToQualifiedName(), typeMap);

            var qualifiedName = XName.Get("hexBinary", NamespaceConstants.SOAP_ENC);
            typeMap = new StreamTypeMap(qualifiedName);
            systemTypeMaps.GetOrAdd(qualifiedName, typeMap);

            qualifiedName = XName.Get("base64", NamespaceConstants.SOAP_ENC);
            typeMap = new StreamTypeMap(qualifiedName);
            systemTypeMaps.GetOrAdd(qualifiedName, typeMap);

            qualifiedName = XName.Get("base64Binary", NamespaceConstants.SOAP_ENC);
            typeMap = new StreamTypeMap(qualifiedName);
            systemTypeMaps.GetOrAdd(qualifiedName, typeMap);

            if (protocol == XRoadProtocol.Version20)
            {
                systemTypeMaps.GetOrAdd(typeof(System.IO.Stream).ToQualifiedName(), typeMap);
                systemTypeMaps.GetOrAdd(typeof(System.IO.FileStream).ToQualifiedName(), typeMap);
                systemTypeMaps.GetOrAdd(typeof(System.IO.MemoryStream).ToQualifiedName(), typeMap);
            }

            qualifiedName = XName.Get("base64binary", NamespaceConstants.XSD);
            typeMap = new StreamTypeMap(qualifiedName);
            systemTypeMaps.GetOrAdd(qualifiedName, typeMap);

            if (protocol != XRoadProtocol.Version20)
            {
                systemTypeMaps.GetOrAdd(typeof(System.IO.Stream).ToQualifiedName(), typeMap);
                systemTypeMaps.GetOrAdd(typeof(System.IO.FileStream).ToQualifiedName(), typeMap);
                systemTypeMaps.GetOrAdd(typeof(System.IO.MemoryStream).ToQualifiedName(), typeMap);
            }

            qualifiedName = XName.Get("testSystem", protocol.GetNamespace());
            var serviceMap = new ConcurrentDictionary<uint, IServiceMap>();
            serviceMap.GetOrAdd(1u, new ServiceMap(qualifiedName, null, null, null, XRoadContentLayoutMode.Strict, false, false));
            serviceMaps.GetOrAdd(qualifiedName, serviceMap);

            qualifiedName = XName.Get("getState", protocol.GetNamespace());
            serviceMap = new ConcurrentDictionary<uint, IServiceMap>();
            serviceMap.GetOrAdd(1u, new ServiceMap(qualifiedName, null, null, new ParameterMap(this, null, null, integerTypeMap, false), XRoadContentLayoutMode.Strict, false, false));
            serviceMaps.GetOrAdd(qualifiedName, serviceMap);

            qualifiedName = XName.Get("listMethods", protocol.GetNamespace());
            serviceMap = new ConcurrentDictionary<uint, IServiceMap>();
            serviceMap.GetOrAdd(1u, new ServiceMap(qualifiedName, null, null, new ParameterMap(this, null, null, stringArrayTypeMap, false), XRoadContentLayoutMode.Strict, false, false));
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
            var serviceInterface = GetServiceInterface(contractAssembly, qualifiedName, dtoVersion);
            if (serviceInterface == null)
                throw XRoadException.UnknownType(qualifiedName.ToString());

            var configuration = protocol.GetContractConfiguration(contractAssembly);
            var parameterMaps = serviceInterface.GetParameters()
                                                .Select(x => CreateParameterMap(qualifiedName.LocalName, configuration.ParameterNameProvider, x, dtoVersion))
                                                .Where(m => m.ParameterInfo.IsParameterInVersion(dtoVersion))
                                                .ToList();

            var resultMap = CreateParameterMap(qualifiedName.LocalName, configuration.ParameterNameProvider, serviceInterface.ReturnParameter, dtoVersion);

            var multipartAttribute = serviceInterface.GetSingleAttribute<XRoadAttachmentAttribute>();
            var hasMultipartRequest = multipartAttribute != null && multipartAttribute.HasMultipartRequest;
            var hasMultipartResponse = multipartAttribute != null && multipartAttribute.HasMultipartResponse;

            var serviceMap = new ServiceMap(qualifiedName, serviceInterface, parameterMaps, resultMap, configuration.OperationContentLayoutMode, hasMultipartRequest, hasMultipartResponse);

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

        private IParameterMap CreateParameterMap(string operationName, IParameterNameProvider parameterNameProvider, ParameterInfo parameterInfo, uint dtoVersion)
        {
            var parameterName = parameterInfo.GetParameterName(parameterNameProvider, operationName);
            var parameterIsOptional = parameterInfo.GetParameterIsOptional();

            var qualifiedTypeName = parameterInfo.GetQualifiedTypeName();
            var typeMap = qualifiedTypeName != null ? GetTypeMap(qualifiedTypeName, dtoVersion) : GetTypeMap(parameterInfo.ParameterType, dtoVersion);

            return new ParameterMap(this, parameterName, parameterInfo, typeMap, parameterIsOptional);
        }

        public ITypeMap GetTypeMapFromXsiType(XmlReader reader, uint dtoVersion, bool undefined = false)
        {
            return GetTypeMap(reader.GetTypeAttributeValue(), dtoVersion, null, undefined);
        }

        public ITypeMap GetTypeMap(Type runtimeType, uint dtoVersion, IDictionary<XName, ITypeMap> partialTypeMaps = null)
        {
            return runtimeType == null ? null : GetTypeMap(runtimeType.ToQualifiedName(), dtoVersion, partialTypeMaps);
        }

        public ITypeMap GetTypeMap(XName qualifiedName, uint dtoVersion, IDictionary<XName, ITypeMap> partialTypeMaps = null, bool undefined = false)
        {
            if (qualifiedName == null)
                return null;

            ITypeMap typeMap;

            if (NamespaceConstants.SOAP_ENC.Equals(qualifiedName.NamespaceName) || NamespaceConstants.XSD.Equals(qualifiedName.NamespaceName) || qualifiedName.NamespaceName.StartsWith("System"))
            {
                if (!systemTypeMaps.TryGetValue(qualifiedName, out typeMap))
                    throw XRoadException.UnknownType(qualifiedName.ToString());
                return typeMap;
            }

            ConcurrentDictionary<uint, ITypeMap> typeMapVersions;
            var fixedQualifiedName = GetTypeMapVersions(qualifiedName, out typeMapVersions);

            if (!typeMapVersions.TryGetValue(dtoVersion, out typeMap) && (partialTypeMaps == null || !partialTypeMaps.TryGetValue(fixedQualifiedName, out typeMap)))
                typeMap = AddTypeMap(typeMapVersions, fixedQualifiedName, dtoVersion, partialTypeMaps, undefined);

            return typeMap;
        }

        private XName GetTypeMapVersions(XName qualifiedName, out ConcurrentDictionary<uint, ITypeMap> typeMapVersions)
        {
            if (typeMaps.TryGetValue(qualifiedName, out typeMapVersions))
                return qualifiedName;

            var fixedQualifiedName = qualifiedName;

            var namespaceBase = protocol.GetProducerNamespaceBase(qualifiedName.NamespaceName);
            if (!string.IsNullOrWhiteSpace(namespaceBase))
            {
                fixedQualifiedName = XName.Get(qualifiedName.LocalName, namespaceBase);
                if (typeMaps.TryGetValue(fixedQualifiedName, out typeMapVersions))
                    return fixedQualifiedName;
            }

            typeMapVersions = typeMaps.GetOrAdd(fixedQualifiedName, new ConcurrentDictionary<uint, ITypeMap>());

            return fixedQualifiedName;
        }

        private ITypeMap AddTypeMap(ConcurrentDictionary<uint, ITypeMap> typeMapVersions, XName qualifiedName, uint dtoVersion, IDictionary<XName, ITypeMap> partialTypeMaps, bool undefined = false)
        {
            ITypeMap typeMap;

            if ((protocol == XRoadProtocol.Version20 || !qualifiedName.NamespaceName.StartsWith("http://")) && qualifiedName.LocalName.EndsWith("[]"))
            {
                var itemTypeName = qualifiedName.LocalName.Substring(0, qualifiedName.LocalName.Length - 2);
                var itemTypeMap = GetTypeMap(XName.Get(itemTypeName, qualifiedName.NamespaceName), dtoVersion, partialTypeMaps);

                var typeMapType = typeof(ArrayTypeMap<>).MakeGenericType(itemTypeMap.RuntimeType);
                typeMap = (ITypeMap)Activator.CreateInstance(typeMapType, this);
            }
            else
            {
                var runtimeType = GetRuntimeType(qualifiedName, undefined);
                if (runtimeType == null)
                    return null;

                if (!runtimeType.IsAbstract && runtimeType.GetConstructor(Type.EmptyTypes) == null)
                    throw XRoadException.NoDefaultConstructorForType(runtimeType.Name);

                if (runtimeType.IsAbstract)
                    typeMap = (ITypeMap)Activator.CreateInstance(typeof(AbstractTypeMap<>).MakeGenericType(runtimeType));
                else
                {
                    var contentLayoutMode = (typeConfigurationProvider?.GetContentLayoutMode(runtimeType)).GetValueOrDefault();
                    if (contentLayoutMode == XRoadContentLayoutMode.Flexible)
                        typeMap = (ITypeMap)Activator.CreateInstance(typeof(AllTypeMap<>).MakeGenericType(runtimeType), this);
                    else
                        typeMap = (ITypeMap)Activator.CreateInstance(typeof(SequenceTypeMap<>).MakeGenericType(runtimeType), this);
                }
            }

            typeMap.DtoVersion = dtoVersion;

            partialTypeMaps = partialTypeMaps ?? new Dictionary<XName, ITypeMap>();
            partialTypeMaps.Add(qualifiedName, typeMap);

            typeMap.InitializeProperties(partialTypeMaps, typeConfigurationProvider);

            partialTypeMaps.Remove(qualifiedName);

            return typeMapVersions.GetOrAdd(typeMap.DtoVersion, typeMap);
        }

        private Type GetRuntimeType(XName qualifiedName, bool undefined)
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
            if (undefined || runtimeType != null)
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
    }
}