using System;
using XRoadLib.Attributes;

namespace XRoadLib.Extensions.ProtoBuf
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
        /// Initializes new operation definition with protocol buffers support.
        /// </summary>
        public XRoadProtoBufServiceAttribute(string name)
            : base(name)
        { }
    }
}