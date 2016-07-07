using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using XRoadLib.Attributes;
using XRoadLib.Extensions;

namespace XRoadLib.Schema
{
    public abstract class ContentDefinition : Definition, IContentDefinition
    {
        public bool MergeContent { get; set; }

        public bool IsNullable { get; set; }

        public bool IsOptional { get; set; }

        public bool UseXop { get; set; }

        public ArrayItemDefinition ArrayItemDefinition { get; set; }

        public int Order { get; set; } = -1;

        public XName TypeName { get; set; }

        public Type RuntimeType { get; set; }

        public abstract string RuntimeName { get; }

        protected void InitializeContentDefinition(ICustomAttributeProvider sourceInfo)
        {
            var elementAttribute = sourceInfo.GetSingleAttribute<XmlElementAttribute>();
            var arrayAttribute = sourceInfo.GetSingleAttribute<XmlArrayAttribute>();
            var arrayItemAttribute = sourceInfo.GetSingleAttribute<XmlArrayItemAttribute>();

            if (elementAttribute != null && (arrayAttribute != null || arrayItemAttribute != null))
                throw new Exception($"Property `{this}` should not define XmlElement and XmlArray(Item) attributes at the same time.");

            var runtimeName = RuntimeName;

            XName qualifiedName = null;
            XName itemQualifiedName = null;

            if (RuntimeType.IsArray)
            {
                if (RuntimeType.GetArrayRank() > 1)
                    throw new Exception($"Property `{this}` declares multi-dimensional array, which is not supported.");

                var localName = (arrayAttribute?.ElementName).GetValueOrDefault(runtimeName);
                var containerName = string.IsNullOrWhiteSpace(localName) ? null : XName.Get(localName, arrayAttribute?.Namespace ?? "");

                if (elementAttribute != null)
                    itemQualifiedName = XName.Get(elementAttribute.ElementName.GetValueOrDefault(runtimeName), elementAttribute.Namespace ?? "");
                else if (arrayItemAttribute != null)
                {
                    qualifiedName = containerName;
                    itemQualifiedName = XName.Get(arrayItemAttribute.ElementName.GetValueOrDefault(runtimeName), arrayItemAttribute.Namespace ?? "");
                }
                else
                {
                    qualifiedName = containerName;
                    itemQualifiedName = XName.Get("item", "");
                }
            }
            else
            {
                if (arrayAttribute != null || arrayItemAttribute != null)
                    throw new Exception($"Property `{this}` should not define XmlArray(Item) attribute, because it's not array type.");
                var name = (elementAttribute?.ElementName).GetValueOrDefault(runtimeName);
                qualifiedName = string.IsNullOrWhiteSpace(name) ? null : XName.Get(name, elementAttribute?.Namespace ?? "");
            }

            var customTypeName = (elementAttribute?.DataType).GetValueOrDefault(arrayItemAttribute?.DataType);

            Name = qualifiedName;
            IsNullable = (elementAttribute?.IsNullable).GetValueOrDefault() || (arrayAttribute?.IsNullable).GetValueOrDefault();
            Order = (elementAttribute?.Order).GetValueOrDefault((arrayAttribute?.Order).GetValueOrDefault());
            UseXop = typeof(Stream).GetTypeInfo().IsAssignableFrom(RuntimeType);
            TypeName = customTypeName != null ? XName.Get(customTypeName, NamespaceConstants.XSD) : null;
            IsOptional = sourceInfo.GetSingleAttribute<XRoadOptionalAttribute>() != null;
            State = DefinitionState.Default;
            Documentation = sourceInfo.GetXRoadTitles().Where(title => !string.IsNullOrWhiteSpace(title.Item2)).ToArray();
            MergeContent = sourceInfo.GetSingleAttribute<XRoadMergeContentAttribute>() != null || sourceInfo.GetSingleAttribute<XmlTextAttribute>() != null;

            if (!RuntimeType.IsArray)
                return;

            MergeContent = MergeContent || elementAttribute != null;

            ArrayItemDefinition = new ArrayItemDefinition(this)
            {
                Name = itemQualifiedName,
                IsNullable = (arrayItemAttribute?.IsNullable).GetValueOrDefault(),
                IsOptional = elementAttribute != null && IsOptional,
                UseXop = typeof(Stream).GetTypeInfo().IsAssignableFrom(RuntimeType.GetElementType()),
                RuntimeType = RuntimeType.GetElementType(),
            };
        }
    }
}