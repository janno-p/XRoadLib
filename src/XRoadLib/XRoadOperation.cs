using System.Collections.Generic;
using XRoadLib.Headers;
using XRoadLib.Serialization;

namespace XRoadLib
{
    public abstract class XRoadOperation<TRequest, TResponse, THeader>
        where THeader : ISoapHeader
    {
        public List<XRoadAttachment> Attachments { get; } = new List<XRoadAttachment>();

        public THeader Header { get; set; }

        public TRequest Request { get; set; }
    }
}