using System.Xml;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Serialization
{
    public interface ISerializerCache
    {
        IServiceMap GetServiceMap(string operationName);

        IServiceMap GetServiceMap(XName qualifiedName);

        ITypeMap GetTypeMapFromXsiType(XmlReader reader);

        ITypeMap GetTypeMap(XName qualifiedName, bool isArray);

        ITypeMap GetTypeMap(Type runtimeType, IDictionary<Type, ITypeMap> partialTypeMaps = null);

        XName GetXmlTypeName(Type type);
    }
}