using System.Xml.Serialization;
using XRoadLib;
using XRoadLib.Attributes;

namespace Calculator.Contract
{
    [XRoadSerializable]
    public class AddRequest : IXRoadRequest<int>
    {
        [XmlElement(Order = 1)]
        public int X { get; set; }

        [XmlElement(Order = 2)]
        public int Y { get; set; }
    }
}