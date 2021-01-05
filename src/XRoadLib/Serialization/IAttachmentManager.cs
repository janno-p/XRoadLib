using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace XRoadLib.Serialization
{
    public interface IAttachmentManager : IDisposable
    {
        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        XRoadAttachment GetAttachment(string contentId);

        IList<XRoadAttachment> AllAttachments { get; }

        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        IEnumerable<XRoadAttachment> MultipartContentAttachments { get; }
    }
}