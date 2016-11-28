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
    /// <summary>
    /// Defines operation method which uses protocol buffers for serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class XRoadProtoBufServiceAttribute : XRoadServiceAttribute
    {
        private const string XROAD_PROTOBUF_SCHEMA = "https://e-rik.github.io/schemas/xroad-protobuf.xsd";

        private static readonly Type serviceMapType = typeof(ProtoBufServiceMap);

        /// <summary>
        /// ServiceMap type which implements operation definition.
        /// </summary>
        public override Type ServiceMapType => serviceMapType;

        /// <summary>
        /// Protocol buffers handles its own serialization, so TypeMaps are not required.
        /// </summary>
        public override bool UseTypeMaps { get; } = false;

        /// <summary>
        /// Type which carries protocol buffers reflection info.
        /// </summary>
        public Type ReflectionType { get; }

        /// <summary>
        /// Initializes new operation definition with protocol buffers support.
        /// </summary>
        public XRoadProtoBufServiceAttribute(string name, Type reflectionType)
            : base(name)
        {
            ReflectionType = reflectionType;
        }

        /// <summary>
        /// Specifies if service extension wants to override default operation definition.
        /// </summary>
        public override void CustomizeOperationDefinition(OperationDefinition definition)
        {
            definition.CopyRequestPartToResponse = false;
        }

        /// <summary>
        /// Specifies if service extension wants to override default request value definition.
        /// </summary>
        public override void CustomizeRequestValueDefinition(RequestValueDefinition definition)
        {
            definition.RuntimeType = typeof(Stream);
            definition.UseXop = true;
            definition.CustomAttributes = new[] { Tuple.Create(XName.Get("schema", XROAD_PROTOBUF_SCHEMA), GetPrototypeName()) };
        }

        /// <summary>
        /// Specifies if service extension wants to override default response value definition.
        /// </summary>
        public override void CustomizeResponseValueDefinition(ResponseValueDefinition definition)
        {
            definition.RuntimeType = typeof(Stream);
            definition.UseXop = true;
            definition.CustomAttributes = new[] { Tuple.Create(XName.Get("schema", XROAD_PROTOBUF_SCHEMA), GetPrototypeName()) };
        }

        /// <summary>
        /// Specifies if service extension wants to override default schema location.
        /// </summary>
        public override string CustomizeSchemaLocation(string namespaceName)
        {
            return namespaceName.Equals(XROAD_PROTOBUF_SCHEMA) ? XROAD_PROTOBUF_SCHEMA : null;
        }

        private string GetPrototypeName()
        {
            var propertyInfo = ReflectionType.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static);
            var descriptor = (FileDescriptor)propertyInfo.GetGetMethod().Invoke(null, new object[0]);
            return descriptor.Name;
        }
    }
}