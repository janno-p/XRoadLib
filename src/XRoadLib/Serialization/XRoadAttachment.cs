using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

namespace XRoadLib.Serialization
{
    /// <summary>
    /// Binary content of X-Road messages.
    /// </summary>
    public class XRoadAttachment : IDisposable
    {
        private const int Base64LineLength = 76;

        private readonly string _contentPath;

        /// <summary>
        /// Underlying stream which contains the content.
        /// </summary>
        public Stream ContentStream { get; private set; }

        /// <summary>
        /// Indicates whether content is serialized as multipart attachement or not.
        /// </summary>
        public bool IsMultipartContent { get; set; }

        /// <summary>
        /// Attachments unique identificator inside the request
        /// which references the multipart content.
        /// </summary>
        public string ContentId { get; }

        /// <summary>
        /// Does the attachment have any content.
        /// </summary>
        public bool HasContent => ContentStream.Length > 0;

        private XRoadAttachment()
        {
            IsMultipartContent = true;
        }

        /// <summary>
        /// Initializes new attachment based on existing stream.
        /// </summary>
        public XRoadAttachment(byte[] contentBytes) : this()
        {
            var contentStream = new MemoryStream(contentBytes);
            ContentId = Convert.ToBase64String(MD5.Create().ComputeHash(contentStream));
            ContentStream = contentStream;
        }

        /// <summary>
        /// Initializes new attachment based on existing stream.
        /// </summary>
        public XRoadAttachment(Stream contentStream) : this()
        {
            ContentId = Convert.ToBase64String(MD5.Create().ComputeHash(contentStream));
            ContentStream = contentStream;
        }

        /// <summary>
        /// Initializes new attachment from temporary file, which is specified
        /// by fullPath.
        /// </summary>
        public XRoadAttachment(string contentId, string fullPath) : this()
        {
            ContentStream = new FileStream(fullPath, FileMode.Create);

            if (string.IsNullOrEmpty(contentId))
                contentId = Convert.ToBase64String(MD5.Create().ComputeHash(ContentStream));

            ContentId = contentId;
            _contentPath = fullPath;
        }

        private void Close()
        {
            ContentStream?.Dispose();
            ContentStream = null;

            // kui path on teada, siis on temp fail ja see tuleb kustutada ..
            if (!string.IsNullOrEmpty(_contentPath) && File.Exists(_contentPath))
                File.Delete(_contentPath);
        }

        /// <summary>
        /// Writes attachments contents to the specificed XML writer object
        /// using base64 encoding.
        /// </summary>
        public void WriteAsBase64(XmlWriter writer)
        {
            ContentStream.Position = 0;

            const int bufferSize = 1000;

            int bytesRead;
            var buffer = new byte[bufferSize];

            while ((bytesRead = ContentStream.Read(buffer, 0, bufferSize)) > 0)
                writer.WriteBase64(buffer, 0, bytesRead);
        }

        /// <summary>
        /// Writes attachments contents to the specificed output stream object
        /// using base64 encoding.
        /// </summary>
        public void WriteAsBase64(TextWriter writer)
        {
            const int bufferSize = (Base64LineLength / 4) * 3;
            var buffer = new byte[bufferSize];

            ContentStream.Position = 0;

            var noContent = true;

            int bytesRead;
            while ((bytesRead = ContentStream.Read(buffer, 0, bufferSize)) > 0)
            {
                noContent = false;
                writer.Write(Convert.ToBase64String(buffer, 0, bytesRead));
                writer.Write(XRoadMessageWriter.NewLine);
            }

            if (noContent)
                writer.Write(XRoadMessageWriter.NewLine);
        }

        /// <summary>
        /// Clean up unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }
}