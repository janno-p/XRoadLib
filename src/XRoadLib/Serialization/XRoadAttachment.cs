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
        private const int BASE64_LINE_LENGTH = 76;

        private readonly string contentID;
        private readonly string contentPath;

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
        public string ContentID { get { return contentID; } }

        /// <summary>
        /// Does the attachment have any content.
        /// </summary>
        public bool HasContent { get { return ContentStream.Length > 0; } }

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
            contentID = Convert.ToBase64String(MD5.Create().ComputeHash(contentStream));
            ContentStream = contentStream;
        }

        /// <summary>
        /// Initializes new attachment based on existing stream.
        /// </summary>
        public XRoadAttachment(Stream contentStream) : this()
        {
            contentID = Convert.ToBase64String(MD5.Create().ComputeHash(contentStream));
            ContentStream = contentStream;
        }

        /// <summary>
        /// Initializes new attachment from temporary file, which is specified
        /// by fullPath.
        /// </summary>
        public XRoadAttachment(string contentID, string fullPath) : this()
        {
            ContentStream = new FileStream(fullPath, FileMode.Create);

            if (string.IsNullOrEmpty(contentID))
                contentID = Convert.ToBase64String(MD5.Create().ComputeHash(ContentStream));

            this.contentID = contentID;

            contentPath = fullPath;
        }

        private void Close()
        {
            ContentStream?.Dispose();
            ContentStream = null;

            // kui path on teada, siis on temp fail ja see tuleb kustutada ..
            if (!string.IsNullOrEmpty(contentPath) && File.Exists(contentPath))
                File.Delete(contentPath);
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
            const int bufferSize = (BASE64_LINE_LENGTH / 4) * 3;
            var buffer = new byte[bufferSize];

            ContentStream.Position = 0;

            var noContent = true;

            int bytesRead;
            while ((bytesRead = ContentStream.Read(buffer, 0, bufferSize)) > 0)
            {
                noContent = false;
                writer.Write(Convert.ToBase64String(buffer, 0, bytesRead));
                writer.Write(XRoadMessageWriter.NEW_LINE);
            }

            if (noContent)
                writer.Write(XRoadMessageWriter.NEW_LINE);
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