using System;
using XRoadLib.Protocols;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public sealed class SerializationContext
    {
        public bool ExcludeNullElement { get; set; }
        public bool FilteringEnabled { get; set; }
        public IXmlTemplate XmlTemplate { get; set; }
        public uint DtoVersion { get; }
        public XRoadMessage Message { get; }

        public IAttachmentManager AttachmentManager => Message;
        public IProtocol Protocol => Message.Protocol;
        public BinaryMode BinaryMode => Message.BinaryContentMode;

        public SerializationContext(XRoadMessage message, uint dtoVersion)
        {
            Message = message;

            if (dtoVersion < 1u)
                throw new ArgumentOutOfRangeException(nameof(dtoVersion));
            DtoVersion = dtoVersion;
        }
    }
}