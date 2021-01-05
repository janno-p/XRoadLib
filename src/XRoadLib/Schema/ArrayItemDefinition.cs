using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using XRoadLib.Attributes;

namespace XRoadLib.Schema
{
    public class ArrayItemDefinition : ParticleDefinition
    {
        public ParticleDefinition Array { get; }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public bool AcceptsAnyName { get; set; }

        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public uint MinOccurs { get; set; }

        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public uint? MaxOccurs { get; set; }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public Type ItemTypeMapType { get; set; }

        public ArrayItemDefinition(ParticleDefinition array, XmlArrayItemAttribute arrayItemAttribute, Type runtimeType, string runtimeName, string targetNamespace, bool defaultQualifiedElement)
        {
            Array = array;

            var xroadArrayItemAttribute = arrayItemAttribute as XRoadXmlArrayItemAttribute;

            MinOccurs = xroadArrayItemAttribute?.MinOccurs ?? 0u;
            MaxOccurs = xroadArrayItemAttribute?.MaxOccurs;

            Content = new SingularContentDefinition(
                this,
                arrayItemAttribute,
                runtimeType,
                runtimeName,
                targetNamespace,
                defaultQualifiedElement
            );
        }
    }
}