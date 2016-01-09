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
        private readonly XRoadProtocol protocol;
        private readonly ConcurrentDictionary<string, Assembly> typeAssemblies = new ConcurrentDictionary<string, Assembly>();
        private readonly ConcurrentDictionary<Assembly, string> assemblyNamespace = new ConcurrentDictionary<Assembly, string>();
        private readonly ConcurrentDictionary<XmlQualifiedName, ConcurrentDictionary<uint, ITypeMap>> customTypeMapCache = new ConcurrentDictionary<XmlQualifiedName, ConcurrentDictionary<uint, ITypeMap>>();
        private readonly ConcurrentDictionary<XmlQualifiedName, ConcurrentDictionary<uint, IServiceMap>> customServiceMaps = new ConcurrentDictionary<XmlQualifiedName, ConcurrentDictionary<uint, IServiceMap>>();
        private readonly ConcurrentDictionary<XmlQualifiedName, ITypeMap> systemTypeMapCache = new ConcurrentDictionary<XmlQualifiedName, ITypeMap>();

        public SerializerCache(XRoadProtocol protocol)
        {
            this.protocol = protocol;

            ITypeMap typeMap = new DateTypeMap();
            systemTypeMapCache.GetOrAdd(new XmlQualifiedName("date", NamespaceHelper.XSD), typeMap);

            typeMap = new DateTimeTypeMap();
            systemTypeMapCache.GetOrAdd(new XmlQualifiedName("dateTime", NamespaceHelper.XSD), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(DateTime).ToQualifiedName(), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(DateTime?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<DateTime>(this);
            if (protocol == XRoadProtocol.Version20)
            {
                systemTypeMapCache.GetOrAdd(new XmlQualifiedName("date[]", NamespaceHelper.XSD), typeMap);
                systemTypeMapCache.GetOrAdd(new XmlQualifiedName("dateTime[]", NamespaceHelper.XSD), typeMap);
            }
            systemTypeMapCache.GetOrAdd(typeof(DateTime[]).ToQualifiedName(), typeMap);

            typeMap = new BooleanTypeMap();
            systemTypeMapCache.GetOrAdd(new XmlQualifiedName("boolean", NamespaceHelper.XSD), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(bool).ToQualifiedName(), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(bool?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<bool>(this);
            if (protocol == XRoadProtocol.Version20)
                systemTypeMapCache.GetOrAdd(new XmlQualifiedName("boolean[]", NamespaceHelper.XSD), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(bool[]).ToQualifiedName(), typeMap);

            typeMap = new FloatTypeMap();
            systemTypeMapCache.GetOrAdd(new XmlQualifiedName("float", NamespaceHelper.XSD), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(float).ToQualifiedName(), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(float?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<float>(this);
            if (protocol == XRoadProtocol.Version20)
                systemTypeMapCache.GetOrAdd(new XmlQualifiedName("float[]", NamespaceHelper.XSD), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(float[]).ToQualifiedName(), typeMap);

            typeMap = new DoubleTypeMap();
            systemTypeMapCache.GetOrAdd(new XmlQualifiedName("double", NamespaceHelper.XSD), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(double).ToQualifiedName(), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(double?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<double>(this);
            if (protocol == XRoadProtocol.Version20)
                systemTypeMapCache.GetOrAdd(new XmlQualifiedName("double[]", NamespaceHelper.XSD), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(double[]).ToQualifiedName(), typeMap);

            typeMap = new DecimalTypeMap();
            systemTypeMapCache.GetOrAdd(new XmlQualifiedName("decimal", NamespaceHelper.XSD), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(decimal).ToQualifiedName(), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(decimal?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<decimal>(this);
            if (protocol == XRoadProtocol.Version20)
                systemTypeMapCache.GetOrAdd(new XmlQualifiedName("decimal[]", NamespaceHelper.XSD), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(decimal[]).ToQualifiedName(), typeMap);

            typeMap = new LongTypeMap();
            systemTypeMapCache.GetOrAdd(new XmlQualifiedName("long", NamespaceHelper.XSD), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(long).ToQualifiedName(), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(long?).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<long>(this);
            if (protocol == XRoadProtocol.Version20)
                systemTypeMapCache.GetOrAdd(new XmlQualifiedName("long[]", NamespaceHelper.XSD), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(long[]).ToQualifiedName(), typeMap);

            typeMap = new IntegerTypeMap();
            var integerTypeMap = typeMap;
            systemTypeMapCache.GetOrAdd(new XmlQualifiedName("integer", NamespaceHelper.XSD), typeMap);
            systemTypeMapCache.GetOrAdd(new XmlQualifiedName("int", NamespaceHelper.XSD), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(int).ToQualifiedName(), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(int?).ToQualifiedName(), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(short).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<int>(this);
            if (protocol == XRoadProtocol.Version20)
            {
                systemTypeMapCache.GetOrAdd(new XmlQualifiedName("integer[]", NamespaceHelper.XSD), typeMap);
                systemTypeMapCache.GetOrAdd(new XmlQualifiedName("int[]", NamespaceHelper.XSD), typeMap);
            }
            systemTypeMapCache.GetOrAdd(typeof(int[]).ToQualifiedName(), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(short[]).ToQualifiedName(), typeMap);

            typeMap = new StringTypeMap();
            systemTypeMapCache.GetOrAdd(new XmlQualifiedName("anyURI", NamespaceHelper.XSD), typeMap);
            systemTypeMapCache.GetOrAdd(new XmlQualifiedName("string", NamespaceHelper.XSD), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(string).ToQualifiedName(), typeMap);

            typeMap = new ArrayTypeMap<string>(this);
            var stringArrayTypeMap = typeMap;
            if (protocol == XRoadProtocol.Version20)
                systemTypeMapCache.GetOrAdd(new XmlQualifiedName("string[]", NamespaceHelper.XSD), typeMap);
            systemTypeMapCache.GetOrAdd(typeof(string[]).ToQualifiedName(), typeMap);

            var qualifiedName = new XmlQualifiedName("hexBinary", NamespaceHelper.SOAP_ENC);
            typeMap = new StreamTypeMap(qualifiedName);
            systemTypeMapCache.GetOrAdd(qualifiedName, typeMap);

            qualifiedName = new XmlQualifiedName("base64", NamespaceHelper.SOAP_ENC);
            typeMap = new StreamTypeMap(qualifiedName);
            systemTypeMapCache.GetOrAdd(qualifiedName, typeMap);

            qualifiedName = new XmlQualifiedName("base64Binary", NamespaceHelper.SOAP_ENC);
            typeMap = new StreamTypeMap(qualifiedName);
            systemTypeMapCache.GetOrAdd(qualifiedName, typeMap);

            if (protocol == XRoadProtocol.Version20)
            {
                systemTypeMapCache.GetOrAdd(typeof(System.IO.Stream).ToQualifiedName(), typeMap);
                systemTypeMapCache.GetOrAdd(typeof(System.IO.FileStream).ToQualifiedName(), typeMap);
                systemTypeMapCache.GetOrAdd(typeof(System.IO.MemoryStream).ToQualifiedName(), typeMap);
            }

            qualifiedName = new XmlQualifiedName("base64binary", NamespaceHelper.XSD);
            typeMap = new StreamTypeMap(qualifiedName);
            systemTypeMapCache.GetOrAdd(qualifiedName, typeMap);

            if (protocol != XRoadProtocol.Version20)
            {
                systemTypeMapCache.GetOrAdd(typeof(System.IO.Stream).ToQualifiedName(), typeMap);
                systemTypeMapCache.GetOrAdd(typeof(System.IO.FileStream).ToQualifiedName(), typeMap);
                systemTypeMapCache.GetOrAdd(typeof(System.IO.MemoryStream).ToQualifiedName(), typeMap);
            }

            qualifiedName = new XmlQualifiedName("testSystem", NamespaceHelper.GetXRoadNamespace(protocol));
            var serviceMap = new ConcurrentDictionary<uint, IServiceMap>();
            serviceMap.GetOrAdd(1u, new ServiceMap(qualifiedName, null, null, true, false, false));
            customServiceMaps.GetOrAdd(qualifiedName, serviceMap);

            qualifiedName = new XmlQualifiedName("getState", NamespaceHelper.GetXRoadNamespace(protocol));
            serviceMap = new ConcurrentDictionary<uint, IServiceMap>();
            serviceMap.GetOrAdd(1u, new ServiceMap(qualifiedName, null, new ParameterMap(this, null, integerTypeMap, false), true, false, false));
            customServiceMaps.GetOrAdd(qualifiedName, serviceMap);

            qualifiedName = new XmlQualifiedName("listMethods", NamespaceHelper.GetXRoadNamespace(protocol));
            serviceMap = new ConcurrentDictionary<uint, IServiceMap>();
            serviceMap.GetOrAdd(1u, new ServiceMap(qualifiedName, null, new ParameterMap(this, null, stringArrayTypeMap, false), true, false, false));
            customServiceMaps.GetOrAdd(qualifiedName, serviceMap);
        }

        public void AddTypeAssembly(Assembly assembly)
        {
            var producerName = assembly.GetXRoadProducerName();

            var @namespace = NamespaceHelper.GetProducerNamespace(producerName, protocol);
            if (typeAssemblies.GetOrAdd(@namespace, assembly) != assembly)
                throw XRoadException.SamaAndmekoguNimiKorduvaltKasutuses(producerName);

            assemblyNamespace.GetOrAdd(assembly, @namespace);
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
            return customServiceMaps.TryGetValue(qualifiedName, out serviceMapVersions)
                ? serviceMapVersions
                : customServiceMaps.GetOrAdd(qualifiedName, new ConcurrentDictionary<uint, IServiceMap>());
        }

        private IServiceMap AddServiceMap(ConcurrentDictionary<uint, IServiceMap> serviceMaps, XmlQualifiedName qualifiedName, uint dtoVersion, MethodInfo methodImpl)
        {
            Assembly typeAssembly;
            if (!typeAssemblies.TryGetValue(qualifiedName.Namespace, out typeAssembly))
                typeAssembly = null;

            var serviceInterface = GetServiceInterface(typeAssembly, qualifiedName, dtoVersion);
            if (serviceInterface == null)
                throw XRoadException.UnknownType(qualifiedName.ToString());

            var parameterMaps = serviceInterface.GetParameters()
                                                .Select((x,i) => new { p = x, m = CreateParameterMap(x, dtoVersion, methodImpl.GetParameters()[i]) })
                                                .Where(x => x.p.IsParameterInVersion(dtoVersion))
                                                .Select(x => x.m)
                                                .ToList();

            var resultName = protocol == XRoadProtocol.Version20 ? null : "value";
            var resultMap = CreateParameterMap(serviceInterface.ReturnParameter, dtoVersion, methodImpl.ReturnParameter, resultName);

            var multipartAttribute = serviceInterface.GetSingleAttribute<XRoadAttachmentAttribute>();
            var hasMultipartRequest = multipartAttribute != null && multipartAttribute.HasMultipartRequest;
            var hasMultipartResponse = multipartAttribute != null && multipartAttribute.HasMultipartResponse;

            var serviceMap = new ServiceMap(qualifiedName, parameterMaps, resultMap, typeAssembly.HasStrictOperationSignature(), hasMultipartRequest, hasMultipartResponse);

            return serviceMaps.GetOrAdd(dtoVersion, serviceMap);
        }

        private MethodInfo GetServiceInterface(Assembly typeAssembly, XmlQualifiedName qualifiedName, uint dtoVersion)
        {
            if (typeAssembly == null)
                return null;

            return typeAssembly.GetTypes()
                               .Where(t => t.IsInterface)
                               .SelectMany(t => t.GetMethods())
                               .SingleOrDefault(x => x.GetServicesInVersion(dtoVersion, true)
                                                      .Any(m => m == qualifiedName.Name));
        }

        private IParameterMap CreateParameterMap(ParameterInfo parameterInfo, uint dtoVersion, ParameterInfo parameterImpl, string parameterName = null)
        {
            var attribute = parameterInfo.GetCustomAttributes(typeof(XRoadParameterAttribute), false).OfType<XRoadParameterAttribute>().SingleOrDefault();

            var usedParameterName = parameterName ??
                                    (protocol == XRoadProtocol.Version20
                                        ? parameterImpl.Name
                                        : !string.IsNullOrWhiteSpace(attribute?.Name) ? attribute.Name : parameterInfo.Name);

            return new ParameterMap(this, usedParameterName, GetTypeMap(parameterInfo.ParameterType, dtoVersion), attribute != null && attribute.IsOptional);
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

            if (NamespaceHelper.SOAP_ENC.Equals(qualifiedName.Namespace) || NamespaceHelper.XSD.Equals(qualifiedName.Namespace) || qualifiedName.Namespace.StartsWith("System"))
            {
                if (!systemTypeMapCache.TryGetValue(qualifiedName, out typeMap))
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
            if (customTypeMapCache.TryGetValue(qualifiedName, out typeMapVersions))
                return qualifiedName;

            var fixedQualifiedName = qualifiedName;

            var namespaceBase = NamespaceHelper.GetNamespaceBase(qualifiedName.Namespace, protocol);
            if (!string.IsNullOrWhiteSpace(namespaceBase))
            {
                fixedQualifiedName = new XmlQualifiedName(qualifiedName.Name, namespaceBase);
                if (customTypeMapCache.TryGetValue(fixedQualifiedName, out typeMapVersions))
                    return fixedQualifiedName;
            }

            typeMapVersions = customTypeMapCache.GetOrAdd(fixedQualifiedName, new ConcurrentDictionary<uint, ITypeMap>());

            return fixedQualifiedName;
        }

        private ITypeMap AddTypeMap(ConcurrentDictionary<uint, ITypeMap> typeMaps, XmlQualifiedName qualifiedName, uint dtoVersion, IDictionary<XmlQualifiedName, ITypeMap> partialTypeMaps, bool undefined = false)
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
                    if (layout == null || layout.Layout == XRoadLayoutKind.Sequence)
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

            return typeMaps.GetOrAdd(typeMap.DtoVersion, typeMap);
        }

        private Type GetRuntimeType(XmlQualifiedName qualifiedName, bool undefined)
        {
            if (!qualifiedName.Namespace.StartsWith("http://"))
                return typeAssemblies.Values
                                     .Select(ass => ass.GetType($"{qualifiedName.Namespace}.{qualifiedName.Name}"))
                                     .SingleOrDefault(type => type != null && type.IsXRoadSerializable());

            Assembly typeAssembly;
            if (!typeAssemblies.TryGetValue(qualifiedName.Namespace, out typeAssembly))
                throw XRoadException.TundmatuNimeruum(qualifiedName.Namespace);

            var runtimeType = typeAssembly.GetTypes()
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
                case "System.Byte": return new XmlQualifiedName("byte", NamespaceHelper.XSD);
                case "System.DateTime": return new XmlQualifiedName("dateTime", NamespaceHelper.XSD);
                case "System.Boolean": return new XmlQualifiedName("boolean", NamespaceHelper.XSD);
                case "System.Single": return new XmlQualifiedName("float", NamespaceHelper.XSD);
                case "System.Double": return new XmlQualifiedName("double", NamespaceHelper.XSD);
                case "System.Decimal": return new XmlQualifiedName("decimal", NamespaceHelper.XSD);
                case "System.Int64": return new XmlQualifiedName("long", NamespaceHelper.XSD);
                case "System.Int32": return new XmlQualifiedName("int", NamespaceHelper.XSD);
                case "System.String": return new XmlQualifiedName("string", NamespaceHelper.XSD);
            }

            string ns;
            if (assemblyNamespace.TryGetValue(type.Assembly, out ns))
                return new XmlQualifiedName(type.Name, ns);

            throw XRoadException.AndmetüübileVastavNimeruumPuudub(type.FullName);
        }
    }
}