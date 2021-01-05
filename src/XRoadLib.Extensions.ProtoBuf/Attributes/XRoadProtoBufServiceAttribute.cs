using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Google.Protobuf.Reflection;
using XRoadLib.Attributes;
using XRoadLib.Extensions.ProtoBuf.Serialization.Mapping;
using XRoadLib.Schema;

namespace XRoadLib.Extensions.ProtoBuf.Attributes
{
    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class XRoadProtoBufServiceAttribute : XRoadServiceAttribute
    {
        private const string XroadProtobufSchema = "https://e-rik.github.io/schemas/xroad-protobuf.xsd";

        private readonly Lazy<ISchemaExporter> _schemaExporter;

        /// <inheritdoc />
        public override Type ServiceMapType { get; } = typeof(ProtoBufServiceMap);

        /// <inheritdoc />
        public override ISchemaExporter SchemaExporter => _schemaExporter.Value;

        /// <inheritdoc />
        public XRoadProtoBufServiceAttribute(string name, Type reflectionType)
            : base(name)
        {
            _schemaExporter = new Lazy<ISchemaExporter>(() => new ProtoBufSchemaExporter(reflectionType));
        }

        private class ProtoBufSchemaExporter : AbstractSchemaExporter
        {
            private readonly Type _reflectionType;

            public override string XRoadPrefix => string.Empty;
            public override string XRoadNamespace => string.Empty;

            public ProtoBufSchemaExporter(Type reflectionType)
                : base(string.Empty)
            {
                _reflectionType = reflectionType;
            }

            public override void ExportOperationDefinition(OperationDefinition operationDefinition)
            {
                operationDefinition.CopyRequestPartToResponse = false;
            }

            public override void ExportRequestDefinition(RequestDefinition requestDefinition)
            {
                requestDefinition.Content.RuntimeType = typeof(Stream);
                requestDefinition.Content.UseXop = true;
                requestDefinition.Content.CustomAttributes = new[] { Tuple.Create(XName.Get("schema", XroadProtobufSchema), GetPrototypeName()) };
            }

            public override void ExportResponseDefinition(ResponseDefinition responseDefinition)
            {
                responseDefinition.Content.RuntimeType = typeof(Stream);
                responseDefinition.Content.UseXop = true;
                responseDefinition.Content.CustomAttributes = new[] { Tuple.Create(XName.Get("schema", XroadProtobufSchema), GetPrototypeName()) };
            }

            public override string ExportSchemaLocation(string namespaceName)
            {
                return namespaceName.Equals(XroadProtobufSchema) ? XroadProtobufSchema : null;
            }

            private string GetPrototypeName()
            {
                var propertyInfo = _reflectionType.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static);
                if (propertyInfo == null)
                    throw new InvalidOperationException();

                var descriptor = (FileDescriptor)propertyInfo.GetGetMethod().Invoke(null, new object[0]);

                return descriptor.Name;
            }
        }
    }
}