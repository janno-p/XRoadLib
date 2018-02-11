using System;
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
    public class XRoadProtoBufServiceAttribute : XRoadServiceAttribute
    {
        private const string XROAD_PROTOBUF_SCHEMA = "https://e-rik.github.io/schemas/xroad-protobuf.xsd";

        private static readonly Type serviceMapType = typeof(ProtoBufServiceMap);

        private readonly Lazy<ISchemaExporter> schemaExporter;

        /// <inheritdoc />
        public override Type ServiceMapType => serviceMapType;

        /// <inheritdoc />
        public override ISchemaExporter SchemaExporter => schemaExporter.Value;

        /// <inheritdoc />
        public XRoadProtoBufServiceAttribute(string name, Type reflectionType)
            : base(name)
        {
            schemaExporter = new Lazy<ISchemaExporter>(() => new ProtoBufSchemaExporter(reflectionType));
        }

        private class ProtoBufSchemaExporter : AbstractSchemaExporter
        {
            private readonly Type reflectionType;

            public override string XRoadPrefix => string.Empty;
            public override string XRoadNamespace => string.Empty;

            public ProtoBufSchemaExporter(Type reflectionType)
                : base(string.Empty)
            {
                this.reflectionType = reflectionType;
            }

            public override void ExportOperationDefinition(OperationDefinition operationDefinition)
            {
                operationDefinition.CopyRequestPartToResponse = false;
            }

            public override void ExportRequestDefinition(RequestDefinition requestDefinition)
            {
                requestDefinition.Content.RuntimeType = typeof(Stream);
                requestDefinition.Content.UseXop = true;
                requestDefinition.Content.CustomAttributes = new[] { Tuple.Create(XName.Get("schema", XROAD_PROTOBUF_SCHEMA), GetPrototypeName()) };
            }

            public override void ExportResponseDefinition(ResponseDefinition responseDefinition)
            {
                responseDefinition.Content.RuntimeType = typeof(Stream);
                responseDefinition.Content.UseXop = true;
                responseDefinition.Content.CustomAttributes = new[] { Tuple.Create(XName.Get("schema", XROAD_PROTOBUF_SCHEMA), GetPrototypeName()) };
            }

            public override string ExportSchemaLocation(string namespaceName)
            {
                return namespaceName.Equals(XROAD_PROTOBUF_SCHEMA) ? XROAD_PROTOBUF_SCHEMA : null;
            }

            private string GetPrototypeName()
            {
                var propertyInfo = reflectionType.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static);
                var descriptor = (FileDescriptor)propertyInfo.GetGetMethod().Invoke(null, new object[0]);
                return descriptor.Name;
            }
        }
    }
}