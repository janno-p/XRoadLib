using System;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public sealed class SerializationContext
    {
        private readonly XRoadMessage message;

        public bool ExcludeNullElement { get; }
        public bool FilteringEnabled { get; }
        public IXmlTemplate XmlTemplate { get; }
        public uint DtoVersion { get; }

        public IAttachmentManager AttachmentManager => message;
        public XRoadProtocol Protocol => message.Protocol;
        public bool IsMultipart => message.IsMultipart;

        public SerializationContext(XRoadMessage message, IXmlTemplate template, uint dtoVersion, bool filteringEnabled, bool excludeNullElement)
        {
            if (dtoVersion < 1U)
                throw new ArgumentOutOfRangeException(nameof(dtoVersion));

            this.message = message;

            DtoVersion = dtoVersion;
            ExcludeNullElement = excludeNullElement;
            FilteringEnabled = filteringEnabled;
            XmlTemplate = template;
        }
    }
}