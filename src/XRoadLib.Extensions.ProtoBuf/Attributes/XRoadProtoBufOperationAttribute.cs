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
    public class XRoadProtoBufOperationAttribute : XRoadOperationAttribute
    {
        private const string XroadProtobufSchema = "https://e-rik.github.io/schemas/xroad-protobuf.xsd";

        private readonly Lazy<ISchemaProvider> _schemaProvider;

        /// <inheritdoc />
        public override Type ServiceMapType { get; } = typeof(ProtoBufServiceMap);

        /// <inheritdoc />
        public override ISchemaProvider SchemaProvider => _schemaProvider.Value;

        /// <inheritdoc />
        public XRoadProtoBufOperationAttribute(string name, Type reflectionType)
            : base(name)
        {
            _schemaProvider = new Lazy<ISchemaProvider>(() => new ProtoBufSchemaExporter(reflectionType));
        }

        private class ProtoBufSchemaExporter : DefaultSchemaProvider
        {
            private readonly Type _reflectionType;

            public override string XRoadPrefix => string.Empty;
            public override string XRoadNamespace => string.Empty;

            public ProtoBufSchemaExporter(Type reflectionType)
                : base(string.Empty, null)
            {
                _reflectionType = reflectionType;
            }

            public override OperationDefinition GetOperationDefinition(Type operationType, XName qualifiedName, uint? version)
            {
                var operationDefinition = base.GetOperationDefinition(operationType, qualifiedName, version);

                operationDefinition.CopyRequestPartToResponse = false;

                return operationDefinition;
            }

            public override RequestDefinition GetRequestDefinition(OperationDefinition operationDefinition)
            {
                var requestDefinition = base.GetRequestDefinition(operationDefinition);
                
                requestDefinition.Content.RuntimeType = typeof(Stream);
                requestDefinition.Content.UseXop = true;
                requestDefinition.Content.CustomAttributes = new[] { Tuple.Create(XName.Get("schema", XroadProtobufSchema), GetPrototypeName()) };

                return requestDefinition;
            }

            public override ResponseDefinition GetResponseDefinition(OperationDefinition operationDefinition, XRoadFaultPresentation? xRoadFaultPresentation)
            {
                var responseDefinition = base.GetResponseDefinition(operationDefinition, xRoadFaultPresentation);
                
                responseDefinition.Content.RuntimeType = typeof(Stream);
                responseDefinition.Content.UseXop = true;
                responseDefinition.Content.CustomAttributes = new[] { Tuple.Create(XName.Get("schema", XroadProtobufSchema), GetPrototypeName()) };

                return responseDefinition;
            }

            public override string GetSchemaLocation(string namespaceName)
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