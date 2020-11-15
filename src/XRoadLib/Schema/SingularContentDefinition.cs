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
        public SingularContentDefinition(ParticleDefinition particle, ICustomAttributeProvider customAttributeProvider, Type runtimeType, string runtimeName, string targetNamespace, bool defaultQualifiedElement)
            : base(particle)
        {
            if (customAttributeProvider.GetXmlArrayAttribute() != null || customAttributeProvider.GetXmlArrayItemAttribute() != null)
                throw new SchemaDefinitionException($"Single content property `{particle} ({runtimeName})` should not use XmlArray or XmlArrayItem attributes in definition.");

            var elementAttribute = customAttributeProvider.GetXmlElementAttribute();
            var xroadElementAttribute = elementAttribute as XRoadXmlElementAttribute;

            Name = XName.Get(
                (elementAttribute?.ElementName).GetStringOrDefault(runtimeName),
                GetQualifiedNamespace(
                    elementAttribute?.Namespace ?? "",
                    elementAttribute?.Form,
                    targetNamespace,
                    defaultQualifiedElement
                )
            );

            IsNullable = (elementAttribute?.IsNullable).GetValueOrDefault();
            Order = (elementAttribute?.Order).GetValueOrDefault(-1);
            UseXop = typeof(Stream).GetTypeInfo().IsAssignableFrom(runtimeType) && (xroadElementAttribute?.UseXop).GetValueOrDefault(true);
            TypeName = (elementAttribute?.DataType).MapNotEmpty(x => XName.Get(x, NamespaceConstants.Xsd));
            IsOptional = xroadElementAttribute?.IsOptional == true;
            State = DefinitionState.Default;
            Documentation = new DocumentationDefinition(customAttributeProvider);
            MergeContent = customAttributeProvider.HasMergeAttribute();
            RuntimeType = runtimeType;
            EmptyTagHandlingMode = xroadElementAttribute?.EmptyTagHandlingMode;
        }

        public SingularContentDefinition(ParticleDefinition particle, XmlArrayItemAttribute arrayItemAttribute, Type runtimeType, string runtimeName, string targetNamespace, bool defaultQualifiedElement)
            : base(particle)
        {
            var xroadArrayItemAttribute = arrayItemAttribute as XRoadXmlArrayItemAttribute;

            Name = XName.Get(
                (arrayItemAttribute?.ElementName).GetStringOrDefault(runtimeName),
                GetQualifiedNamespace(
                    arrayItemAttribute?.Namespace ?? "",
                    arrayItemAttribute?.Form,
                    targetNamespace,
                    defaultQualifiedElement
                )
            );

            IsNullable = (arrayItemAttribute?.IsNullable).GetValueOrDefault();
            UseXop = typeof(Stream).GetTypeInfo().IsAssignableFrom(runtimeType) && (xroadArrayItemAttribute?.UseXop).GetValueOrDefault(true);
            TypeName = (arrayItemAttribute?.DataType).MapNotEmpty(x => XName.Get(x, NamespaceConstants.Xsd));
            State = DefinitionState.Default;
            RuntimeType = runtimeType;
            EmptyTagHandlingMode = xroadArrayItemAttribute?.EmptyTagHandlingMode;
        }
    }
}