using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using XRoadLib.Attributes;
using XRoadLib.Extensions;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Serialization
{
    public sealed class SerializerCache : ISerializerCache
    {
        private readonly Assembly contractAssembly;
        private readonly XRoadProtocol protocol;
        private readonly string producerNamespace;

        private readonly ConcurrentDictionary<XmlQualifiedName, ConcurrentDictionary<uint, ITypeMap>> typeMaps = new ConcurrentDictionary<XmlQualifiedName, ConcurrentDictionary<uint, ITypeMap>>();
        private readonly ConcurrentDictionary<XmlQualifiedName, ConcurrentDictionary<uint, IServiceMap>> serviceMaps = new ConcurrentDictionary<XmlQualifiedName, ConcurrentDictionary<uint, IServiceMap>>();
        private readonly ConcurrentDictionary<XmlQualifiedName, ITypeMap> systemTypeMaps = new ConcurrentDictionary<XmlQualifiedName, ITypeMap>();

        public string ProducerName { get; }

        public SerializerCache(Assembly contractAssembly, XRoadProtocol protocol)
        {
            this.contractAssembly = contractAssembly;
            this.protocol = protocol;

            ProducerName = protocol.GetProducerName(contractAssembly);
            producerNamespace = protocol.GetProducerNamespace(ProducerName);

            ITypeMap typeMap = new DateTypeMap();
            systemTypeMaps.GetOrAdd(new XmlQualifiedName("date", NamespaceConstants.XSD), typeMap);

            typeMap = new DateTimeTypeMap();
            systemTypeMaps.GetOrAdd(new XmlQualifiedName("dateTime", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(DateTime).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(DateTime?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<DateTime>(this);
            if (protocol == XRoadProtocol.Version20)
            {
                systemTypeMaps.GetOrAdd(new XmlQualifiedName("date[]", NamespaceConstants.XSD), typeMap);
                systemTypeMaps.GetOrAdd(new XmlQualifiedName("dateTime[]", NamespaceConstants.XSD), typeMap);
            }
            systemTypeMaps.GetOrAdd(typeof(DateTime[]).ToQualifiedName(), typeMap);

            typeMap = new BooleanTypeMap();
            systemTypeMaps.GetOrAdd(new XmlQualifiedName("boolean", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(bool).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(bool?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<bool>(this);
            if (protocol == XRoadProtocol.Version20)
                systemTypeMaps.GetOrAdd(new XmlQualifiedName("boolean[]", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(bool[]).ToQualifiedName(), typeMap);

            typeMap = new FloatTypeMap();
            systemTypeMaps.GetOrAdd(new XmlQualifiedName("float", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(float).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(float?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<float>(this);
            if (protocol == XRoadProtocol.Version20)
                systemTypeMaps.GetOrAdd(new XmlQualifiedName("float[]", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(float[]).ToQualifiedName(), typeMap);

            typeMap = new DoubleTypeMap();
            systemTypeMaps.GetOrAdd(new XmlQualifiedName("double", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(double).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(double?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<double>(this);
            if (protocol == XRoadProtocol.Version20)
                systemTypeMaps.GetOrAdd(new XmlQualifiedName("double[]", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(double[]).ToQualifiedName(), typeMap);

            typeMap = new DecimalTypeMap();
            systemTypeMaps.GetOrAdd(new XmlQualifiedName("decimal", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(decimal).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(decimal?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<decimal>(this);
            if (protocol == XRoadProtocol.Version20)
                systemTypeMaps.GetOrAdd(new XmlQualifiedName("decimal[]", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(decimal[]).ToQualifiedName(), typeMap);

            typeMap = new LongTypeMap();
            systemTypeMaps.GetOrAdd(new XmlQualifiedName("long", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(long).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(long?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<long>(this);
            if (protocol == XRoadProtocol.Version20)
                systemTypeMaps.GetOrAdd(new XmlQualifiedName("long[]", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(long[]).ToQualifiedName(), typeMap);

            typeMap = new IntegerTypeMap();
            var integerTypeMap = typeMap;
            systemTypeMaps.GetOrAdd(new XmlQualifiedName("integer", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(new XmlQualifiedName("int", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(int).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(int?).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(short).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<int>(this);
            if (protocol == XRoadProtocol.Version20)
            {
                systemTypeMaps.GetOrAdd(new XmlQualifiedName("integer[]", NamespaceConstants.XSD), typeMap);
                systemTypeMaps.GetOrAdd(new XmlQualifiedName("int[]", NamespaceConstants.XSD), typeMap);
            }
            systemTypeMaps.GetOrAdd(typeof(int[]).ToQualifiedName(), typeMap);
            systemTypeMaps.GetOrAdd(typeof(short[]).ToQualifiedName(), typeMap);

            typeMap = new StringTypeMap();
            systemTypeMaps.GetOrAdd(new XmlQualifiedName("anyURI", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(new XmlQualifiedName("string", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(string).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<string>(this);
            var stringArrayTypeMap = typeMap;
            if (protocol == XRoadProtocol.Version20)
                systemTypeMaps.GetOrAdd(new XmlQualifiedName("string[]", NamespaceConstants.XSD), typeMap);
            systemTypeMaps.GetOrAdd(typeof(string[]).ToQualifiedName(), typeMap);

            var qualifiedName = new XmlQualifiedName("hexBinary", NamespaceConstants.SOAP_ENC);
            typeMap = new StreamTypeMap(qualifiedName);
            systemTypeMaps.GetOrAdd(qualifiedName, typeMap);

            qualifiedName = new XmlQualifiedName("base64", NamespaceConstants.SOAP_ENC);
            typeMap = new StreamTypeMap(qualifiedName);
            systemTypeMaps.GetOrAdd(qualifiedName, typeMap);

            qualifiedName = new XmlQualifiedName("base64Binary", NamespaceConstants.SOAP_ENC);
            typeMap = new StreamTypeMap(qualifiedName);
            systemTypeMaps.GetOrAdd(qualifiedName, typeMap);

            if (protocol == XRoadProtocol.Version20)
            {
                systemTypeMaps.GetOrAdd(typeof(System.IO.Stream).ToQualifiedName(), typeMap);
                systemTypeMaps.GetOrAdd(typeof(System.IO.FileStream).ToQualifiedName(), typeMap);
                systemTypeMaps.GetOrAdd(typeof(System.IO.MemoryStream).ToQualifiedName(), typeMap);
            }

            qualifiedName = new XmlQualifiedName("base64binary", NamespaceConstants.XSD);
            typeMap = new StreamTypeMap(qualifiedName);
            systemTypeMaps.GetOrAdd(qualifiedName, typeMap);

            if (protocol != XRoadProtocol.Version20)
            {
                systemTypeMaps.GetOrAdd(typeof(System.IO.Stream).ToQualifiedName(), typeMap);
                systemTypeMaps.GetOrAdd(typeof(System.IO.FileStream).ToQualifiedName(), typeMap);
                systemTypeMaps.GetOrAdd(typeof(System.IO.MemoryStream).ToQualifiedName(), typeMap);
            }

            qualifiedName = new XmlQualifiedName("testSystem", protocol.GetNamespace());
            var serviceMap = new ConcurrentDictionary<uint, IServiceMap>();
            serviceMap.GetOrAdd(1u, new ServiceMap(qualifiedName, null, null, XRoadContentLayoutMode.Strict, false, false));
            serviceMaps.GetOrAdd(qualifiedName, serviceMap);

            qualifiedName = new XmlQualifiedName("getState", protocol.GetNamespace());
            serviceMap = new ConcurrentDictionary<uint, IServiceMap>();
            serviceMap.GetOrAdd(1u, new ServiceMap(qualifiedName, null, new ParameterMap(this, null, null, integerTypeMap, false), XRoadContentLayoutMode.Strict, false, false));
            serviceMaps.GetOrAdd(qualifiedName, serviceMap);

            qualifiedName = new XmlQualifiedName("listMethods", protocol.GetNamespace());
            serviceMap = new ConcurrentDictionary<uint, IServiceMap>();
            serviceMap.GetOrAdd(1u, new ServiceMap(qualifiedName, null, new ParameterMap(this, null, null, stringArrayTypeMap, false), XRoadContentLayoutMode.Strict, false, false));
            serviceMaps.GetOrAdd(qualifiedName, serviceMap);
        }

        public IServiceMap GetServiceMap(XmlQualifiedName qualifiedName, uint dtoVersion, MethodInfo methodImpl)
        {
            if (qualifiedName == null)
                return null;

            var serviceMapVersions = GetServiceMapVersions(qualifiedName);

            IServiceMap serviceMap;
            if (!serviceMapVersions.TryGetValue(dtoVersion, out serviceMap))
                serviceMap = AddServiceMap(serviceMapVersions, qualifiedName, dtoVersion, methodImpl);

            return serviceMap;
        }

        private ConcurrentDictionary<uint, IServiceMap> GetServiceMapVersions(XmlQualifiedName qualifiedName)
        {
            ConcurrentDictionary<uint, IServiceMap> serviceMapVersions;
            return serviceMaps.TryGetValue(qualifiedName, out serviceMapVersions)
                ? serviceMapVersions
                : serviceMaps.GetOrAdd(qualifiedName, new ConcurrentDictionary<uint, IServiceMap>());
        }

        private IServiceMap AddServiceMap(ConcurrentDictionary<uint, IServiceMap> serviceMapVersions, XmlQualifiedName qualifiedName, uint dtoVersion, MethodInfo methodImpl)
        {
            var serviceInterface = GetServiceInterface(contractAssembly, qualifiedName, dtoVersion);
            if (serviceInterface == null)
                throw XRoadException.UnknownType(qualifiedName.ToString());

            var configuration = protocol.GetContractConfiguration(contractAssembly);
            var parameterMaps = serviceInterface.GetParameters()
                                                .Select((x,i) => new { p = x, m = CreateParameterMap(configuration.ParameterNameProvider, x, dtoVersion, methodImpl.GetParameters()[i]) })
                                                .Where(x => x.p.IsParameterInVersion(dtoVersion))
                                                .Select(x => x.m)
                                                .ToList();

            var resultName = protocol == XRoadProtocol.Version20 ? null : "value";
            var resultMap = CreateParameterMap(null, serviceInterface.ReturnParameter, dtoVersion, methodImpl.ReturnParameter, resultName);

            var multipartAttribute = serviceInterface.GetSingleAttribute<XRoadAttachmentAttribute>();
            var hasMultipartRequest = multipartAttribute != null && multipartAttribute.HasMultipartRequest;
            var hasMultipartResponse = multipartAttribute != null && multipartAttribute.HasMultipartResponse;

            var serviceMap = new ServiceMap(qualifiedName, parameterMaps, resultMap, configuration.OperationContentLayoutMode, hasMultipartRequest, hasMultipartResponse);

            return serviceMapVersions.GetOrAdd(dtoVersion, serviceMap);
        }

        private MethodInfo GetServiceInterface(Assembly typeAssembly, XmlQualifiedName qualifiedName, uint dtoVersion)
        {
            return typeAssembly?.GetTypes()
                                .Where(t => t.IsInterface)
                                .SelectMany(t => t.GetMethods())
                                .SingleOrDefault(x => x.GetServicesInVersion(dtoVersion, true)
                                                       .Any(m => m == qualifiedName.Name));
        }

        private IParameterMap CreateParameterMap(IParameterNameProvider parameterNameProvider, ParameterInfo parameterInfo, uint dtoVersion, ParameterInfo parameterImpl, string parameterName = null)
        {
            var attribute = parameterInfo.GetCustomAttributes(typeof(XRoadParameterAttribute), false).OfType<XRoadParameterAttribute>().SingleOrDefault();

            var usedParameterName = parameterName ??
                                    (parameterNameProvider != null
                                        ? parameterNameProvider.GetParameterName(parameterInfo, parameterImpl)
                                        : !string.IsNullOrWhiteSpace(attribute?.Name) ? attribute.Name : parameterInfo.Name);

            var qualifiedTypeName = parameterInfo.GetQualifiedTypeName();
            var typeMap = qualifiedTypeName != null ? GetTypeMap(qualifiedTypeName, dtoVersion) : GetTypeMap(parameterInfo.ParameterType, dtoVersion);

            return new ParameterMap(this, usedParameterName, parameterImpl, typeMap, attribute != null && attribute.IsOptional);
        }

        public ITypeMap GetTypeMapFromXsiType(XmlReader reader, uint dtoVersion, bool undefined = false)
        {
            return GetTypeMap(reader.GetTypeAttributeValue(), dtoVersion, null, undefined);
        }

        public ITypeMap GetTypeMap(Type runtimeType, uint dtoVersion, IDictionary<XmlQualifiedName, ITypeMap> partialTypeMaps = null)
        {
            return runtimeType == null ? null : GetTypeMap(runtimeType.ToQualifiedName(), dtoVersion, partialTypeMaps);
        }

        public ITypeMap GetTypeMap(XmlQualifiedName qualifiedName, uint dtoVersion, IDictionary<XmlQualifiedName, ITypeMap> partialTypeMaps = null, bool undefined = false)
        {
            if (qualifiedName == null)
                return null;

            ITypeMap typeMap;

            if (NamespaceConstants.SOAP_ENC.Equals(qualifiedName.Namespace) || NamespaceConstants.XSD.Equals(qualifiedName.Namespace) || qualifiedName.Namespace.StartsWith("System"))
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

        private XmlQualifiedName GetTypeMapVersions(XmlQualifiedName qualifiedName, out ConcurrentDictionary<uint, ITypeMap> typeMapVersions)
        {
            if (typeMaps.TryGetValue(qualifiedName, out typeMapVersions))
                return qualifiedName;

            var fixedQualifiedName = qualifiedName;

            var namespaceBase = protocol.GetProducerNamespaceBase(qualifiedName.Namespace);
            if (!string.IsNullOrWhiteSpace(namespaceBase))
            {
                fixedQualifiedName = new XmlQualifiedName(qualifiedName.Name, namespaceBase);
                if (typeMaps.TryGetValue(fixedQualifiedName, out typeMapVersions))
                    return fixedQualifiedName;
            }

            typeMapVersions = typeMaps.GetOrAdd(fixedQualifiedName, new ConcurrentDictionary<uint, ITypeMap>());

            return fixedQualifiedName;
        }

        private ITypeMap AddTypeMap(ConcurrentDictionary<uint, ITypeMap> typeMapVersions, XmlQualifiedName qualifiedName, uint dtoVersion, IDictionary<XmlQualifiedName, ITypeMap> partialTypeMaps, bool undefined = false)
        {
            ITypeMap typeMap;

            if ((protocol == XRoadProtocol.Version20 || !qualifiedName.Namespace.StartsWith("http://")) && qualifiedName.Name.EndsWith("[]"))
            {
                var itemTypeName = qualifiedName.Name.Substring(0, qualifiedName.Name.Length - 2);
                var itemTypeMap = GetTypeMap(new XmlQualifiedName(itemTypeName, qualifiedName.Namespace), dtoVersion, partialTypeMaps);

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
                    var layout = runtimeType.GetLayoutAttribute(protocol);
                    if (layout == null || layout.ContentLayout == XRoadContentLayoutMode.Strict)
                        typeMap = (ITypeMap)Activator.CreateInstance(typeof(SequenceTypeMap<>).MakeGenericType(runtimeType), this);
                    else
                        typeMap = (ITypeMap)Activator.CreateInstance(typeof(AllTypeMap<>).MakeGenericType(runtimeType), this);
                }
            }

            typeMap.DtoVersion = dtoVersion;

            partialTypeMaps = partialTypeMaps ?? new Dictionary<XmlQualifiedName, ITypeMap>();
            partialTypeMaps.Add(qualifiedName, typeMap);

            typeMap.InitializeProperties(partialTypeMaps, protocol);

            partialTypeMaps.Remove(qualifiedName);

            return typeMapVersions.GetOrAdd(typeMap.DtoVersion, typeMap);
        }

        private Type GetRuntimeType(XmlQualifiedName qualifiedName, bool undefined)
        {
            if (!qualifiedName.Namespace.StartsWith("http://"))
            {
                var type = contractAssembly.GetType($"{qualifiedName.Namespace}.{qualifiedName.Name}");
                return type != null && type.IsXRoadSerializable() ? type : null;
            }

            if (!producerNamespace.Equals(qualifiedName.Namespace))
                throw XRoadException.TundmatuNimeruum(qualifiedName.Namespace);

            var runtimeType = contractAssembly.GetTypes()
                                              .Where(type => type.Name.Equals(qualifiedName.Name))
                                              .SingleOrDefault(type => type.IsXRoadSerializable());
            if (undefined || runtimeType != null)
                return runtimeType;

            throw XRoadException.UnknownType(qualifiedName.ToString());
        }
        
        public XmlQualifiedName GetXmlTypeName(Type type)
        {
            if (type.IsNullable())
                return GetXmlTypeName(Nullable.GetUnderlyingType(type));

            switch (type.FullName)
            {
                case "System.Byte": return new XmlQualifiedName("byte", NamespaceConstants.XSD);
                case "System.DateTime": return new XmlQualifiedName("dateTime", NamespaceConstants.XSD);
                case "System.Boolean": return new XmlQualifiedName("boolean", NamespaceConstants.XSD);
                case "System.Single": return new XmlQualifiedName("float", NamespaceConstants.XSD);
                case "System.Double": return new XmlQualifiedName("double", NamespaceConstants.XSD);
                case "System.Decimal": return new XmlQualifiedName("decimal", NamespaceConstants.XSD);
                case "System.Int64": return new XmlQualifiedName("long", NamespaceConstants.XSD);
                case "System.Int32": return new XmlQualifiedName("int", NamespaceConstants.XSD);
                case "System.String": return new XmlQualifiedName("string", NamespaceConstants.XSD);
            }

            if (type.Assembly == contractAssembly)
                return new XmlQualifiedName(type.Name, producerNamespace);

            throw XRoadException.AndmetüübileVastavNimeruumPuudub(type.FullName);
        }
    }
}