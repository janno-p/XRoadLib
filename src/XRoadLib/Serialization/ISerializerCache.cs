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

        ITypeMap GetTypeMapFromXsiType(XmlReader reader, uint dtoVersion);

        ITypeMap GetTypeMap(XName qualifiedName, bool isArray, uint dtoVersion);

        ITypeMap GetTypeMap(Type runtimeType, uint dtoVersion, IDictionary<Type, ITypeMap> partialTypeMaps = null);

        Tuple<ITypeMap, ITypeMap> GetTypeMaps(XName qualifiedName, uint dtoVersion);

        XName GetXmlTypeName(Type type);
    }
}