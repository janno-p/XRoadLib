using System.Xml;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Serialization
{
    public interface ISerializerCache
    {
        string ProducerNamespace { get; }

        IServiceMap GetServiceMap(string operationName, uint dtoVersion);

        IServiceMap GetServiceMap(XName qualifiedName, uint dtoVersion);

        ITypeMap GetTypeMapFromXsiType(XmlReader reader, uint dtoVersion, bool undefined = false);

        ITypeMap GetTypeMap(Type runtimeType, uint dtoVersion, IDictionary<XName, ITypeMap> partialTypeMaps = null);

        ITypeMap GetTypeMap(XName qualifiedName, uint dtoVersion, IDictionary<XName, ITypeMap> partialTypeMaps = null, bool undefined = false);

        XName GetXmlTypeName(Type type);
    }
}