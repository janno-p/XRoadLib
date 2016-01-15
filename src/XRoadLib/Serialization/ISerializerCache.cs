using System.Reflection;
using System.Xml;
using System;
using System.Collections.Generic;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Serialization
{
    public interface ISerializerCache
    {
        string ProducerName { get; }

        IServiceMap GetServiceMap(XmlQualifiedName qualifiedName, uint dtoVersion, MethodInfo methodImpl);

        ITypeMap GetTypeMapFromXsiType(XmlReader reader, uint dtoVersion, bool undefined = false);

        ITypeMap GetTypeMap(Type runtimeType, uint dtoVersion, IDictionary<XmlQualifiedName, ITypeMap> partialTypeMaps = null);

        ITypeMap GetTypeMap(XmlQualifiedName qualifiedName, uint dtoVersion, IDictionary<XmlQualifiedName, ITypeMap> partialTypeMaps = null, bool undefined = false);

        XmlQualifiedName GetXmlTypeName(Type type);
    }
}