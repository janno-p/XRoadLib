using System;
using System.Collections.Generic;
using System.Linq;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions.Http
{
    public class XRoadResponse<TResult> : IDisposable
    {
        private readonly IList<XRoadAttachment> _attachments;

        public TResult Result { get; }
        public IEnumerable<XRoadAttachment> Attachments => _attachments.ToList();

        public XRoadResponse(TResult result, IList<XRoadAttachment> attachments)
        {
            Result = result;
            _attachments = attachments ?? new List<XRoadAttachment>();
        }

        public void Dispose()
        {
            foreach (var attachment in _attachments)
                attachment.Dispose();

            _attachments.Clear();
        }
    }
}