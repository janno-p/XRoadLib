using System;
using System.Xml.Serialization;
using XRoadLib.Attributes;

namespace XRoadLib.Schema
{
    public class ArrayItemDefinition : ParticleDefinition
    {
        public ParticleDefinition Array { get; }

        public bool AcceptsAnyName { get; set; }

        public uint MinOccurs { get; set; }

        public uint? MaxOccurs { get; set; }

        public Type ItemTypeMapType { get; set; }

        public ArrayItemDefinition(ParticleDefinition array, XmlArrayItemAttribute arrayItemAttribute, Type runtimeType, string runtimeName)
        {
            Array = array;

            var xroadArrayItemAttribute = arrayItemAttribute as XRoadXmlArrayItemAttribute;

            MinOccurs = xroadArrayItemAttribute?.MinOccurs ?? 0u;
            MaxOccurs = xroadArrayItemAttribute?.MaxOccurs;

            Content = new SingularContentDefinition(
                this,
                arrayItemAttribute,
                runtimeType,
                runtimeName
            );
        }
    }
}