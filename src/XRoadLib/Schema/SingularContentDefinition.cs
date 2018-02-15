using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using XRoadLib.Attributes;
using XRoadLib.Extensions;

namespace XRoadLib.Schema
{
    public class SingularContentDefinition : ContentDefinition
    {
        public SingularContentDefinition(ParticleDefinition particle, ICustomAttributeProvider customAttributeProvider, Type runtimeType, string runtimeName)
            : base(particle)
        {
            if (customAttributeProvider.GetXmlArrayAttribute() != null || customAttributeProvider.GetXmlArrayItemAttribute() != null)
                throw new Exception($"Singe content property `{particle} ({runtimeName})` should not use XmlArray or XmlArrayItem attributes in definition.");

            var elementAttribute = customAttributeProvider.GetXmlElementAttribute();
            var xroadElementAttribute = elementAttribute as XRoadXmlElementAttribute;

            Name = XName.Get((elementAttribute?.ElementName).GetStringOrDefault(runtimeName), elementAttribute?.Namespace ?? "");
            IsNullable = (elementAttribute?.IsNullable).GetValueOrDefault();
            Order = (elementAttribute?.Order).GetValueOrDefault(-1);
            UseXop = typeof(Stream).GetTypeInfo().IsAssignableFrom(runtimeType);
            TypeName = (elementAttribute?.DataType).MapNotEmpty(x => XName.Get(x, NamespaceConstants.XSD));
            IsOptional = xroadElementAttribute?.IsOptional == true;
            State = DefinitionState.Default;
            Documentation = new DocumentationDefinition(customAttributeProvider);
            MergeContent = customAttributeProvider.HasMergeAttribute();
            RuntimeType = runtimeType;
        }

        public SingularContentDefinition(ParticleDefinition particle, XmlArrayItemAttribute arrayItemAttribute, Type runtimeType, string runtimeName)
            : base(particle)
        {
            Name = XName.Get((arrayItemAttribute?.ElementName).GetStringOrDefault(runtimeName), arrayItemAttribute?.Namespace ?? "");
            IsNullable = (arrayItemAttribute?.IsNullable).GetValueOrDefault();
            UseXop = typeof(Stream).GetTypeInfo().IsAssignableFrom(runtimeType);
            TypeName = (arrayItemAttribute?.DataType).MapNotEmpty(x => XName.Get(x, NamespaceConstants.XSD));
            State = DefinitionState.Default;
            RuntimeType = runtimeType;
        }
    }
}