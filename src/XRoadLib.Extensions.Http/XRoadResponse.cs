using System;
using System.Collections.Generic;
using System.Linq;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions.Http
{
    public class XRoadResponse : IDisposable
    {
        private readonly IList<XRoadAttachment> _attachments;

        public object Result { get; }
        public IEnumerable<XRoadAttachment> Attachments => _attachments.ToList();

        public XRoadResponse(object result, IList<XRoadAttachment> attachments)
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