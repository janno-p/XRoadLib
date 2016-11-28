using System;
using XRoadLib.Attributes;
using XRoadLib.Extensions.ProtoBuf.Serialization.Mapping;

namespace XRoadLib.Extensions.ProtoBuf.Attributes
{
    /// <summary>
    /// Defines operation method which uses protocol buffers for serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class XRoadProtoBufServiceAttribute : XRoadServiceAttribute
    {
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
        /// Initializes new operation definition with protocol buffers support.
        /// </summary>
        public XRoadProtoBufServiceAttribute(string name)
            : base(name)
        { }
    }
}