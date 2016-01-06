using System;
using System.Collections.Generic;

namespace XRoadLib.Serialization
{
    public interface IAttachmentManager : IDisposable
    {
        XRoadAttachment GetAttachment(string contentId);

        IList<XRoadAttachment> AllAttachments { get; }

        IEnumerable<XRoadAttachment> MultipartContentAttachments { get; }
    }
}