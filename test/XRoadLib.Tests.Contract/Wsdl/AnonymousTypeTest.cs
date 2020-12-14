using System.Xml.Serialization;
using XRoadLib.Attributes;

namespace XRoadLib.Tests.Contract.Wsdl
{
    [XRoadSerializable]
    [XmlType(AnonymousType = true)]
    public class AnonymousType
    {
        public string Property1 { get; set; }
        public string Property2 { get; set; }
        public string Property3 { get; set; }
    }

    [XRoadSerializable]
    public class ContainerType
    {
        public AnonymousType AnonymousProperty { get; set; }
        public string KnownProperty { get; set; }
    }
}