using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Extensions.Http.Extensions
{
    public static class XRoadMessageExtensions
    {
        public static HttpContent GetHttpContent(this XRoadMessage message, IMessageFormatter messageFormatter)
        {
            message.ContentStream.Position = 0;

            if (!message.MultipartContentAttachments.Any())
                return new StreamContent(message.ContentStream);

            var httpContent = new MultipartContent("related");

            var type = message.BinaryMode == BinaryMode.Xml ? ContentTypes.Xop : messageFormatter.ContentType;
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("type", $@"""{type}"""));

            var contentId = Convert.ToBase64String(MD5.Create().ComputeHash(message.ContentStream));
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("start", $@"""{contentId}"""));

            if (message.BinaryMode == BinaryMode.Xml)
                httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("start-info", $@"""{messageFormatter.ContentType}"""));

            httpContent.Headers.Add("MIME-Version", "1.0");

            message.ContentStream.Position = 0;

            var xmlContent = new StreamContent(message.ContentStream);

            var xmlMediaType = message.BinaryMode == BinaryMode.Attachment ? messageFormatter.ContentType : ContentTypes.Xop;
            xmlContent.Headers.ContentType = new MediaTypeHeaderValue(xmlMediaType) { CharSet = XRoadEncoding.Utf8.WebName };
            if (message.BinaryMode != BinaryMode.Attachment)
                xmlContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("type", $@"""{messageFormatter.ContentType}"""));

            xmlContent.Headers.Add("Content-Transfer-Encoding", "8bit");
            xmlContent.Headers.Add("Content-ID", $"<{contentId.Trim('<', '>', ' ')}>");

            httpContent.Add(xmlContent);

            foreach (var attachment in message.MultipartContentAttachments)
            {
                attachment.ContentStream.Position = 0;

                if (message.BinaryMode == BinaryMode.Xml)
                {
                    var attachmentContent = new StreamContent(attachment.ContentStream);

                    attachmentContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    attachmentContent.Headers.Add("Content-Transfer-Encoding", "binary");
                    attachmentContent.Headers.Add("Content-ID", $"<{attachment.ContentId.Trim('<', '>', ' ')}");

                    httpContent.Add(attachmentContent);
                }
                else
                {
                    var attachmentContent = new Base64EncodedContent(attachment.ContentStream);

                    attachmentContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    attachmentContent.Headers.Add("Content-Disposition", "attachment; filename=notAnswering");
                    attachmentContent.Headers.Add("Content-ID", $"<{attachment.ContentId.Trim('<', '>', ' ')}");

                    httpContent.Add(attachmentContent);
                }
            }

            return httpContent;
        }
    }
}