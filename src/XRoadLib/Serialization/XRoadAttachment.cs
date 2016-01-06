using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

namespace XRoadLib.Serialization
{
    public class XRoadAttachment : IDisposable
    {
        private readonly string contentID;
        private readonly string contentPath;

        public Stream ContentStream { get; private set; }

        public bool IsMultipartContent { get; set; }

        /// <summary>Manuse unikaalne tunnus päringu sees, mille järgi viidatakse manusele SOAP sõnumist.</summary>
        public string ContentID { get { return contentID; } }

        public bool HasContent { get { return ContentStream.Length > 0; } }

        private XRoadAttachment()
        {
            IsMultipartContent = true;
        }

        /// <summary>
        /// Seda interface't kasutatakse süsteemis olemasoleva faili esitamisel attachement'ina
        /// St. mitte siis kui klient uploadib faili, vadi siis, kui ta downloadib
        /// </summary>
        public XRoadAttachment(Stream contentStream) : this()
        {
            contentID = Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(contentStream));
            ContentStream = contentStream;
        }

        /// <summary>Seda interfacet kasutatakse faili süsteemi uploadimisel</summary>
        public XRoadAttachment(string contentID, string fullPath) : this()
        {
            ContentStream = new FileStream(fullPath, FileMode.Create);

            if (string.IsNullOrEmpty(contentID))
                contentID = Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(ContentStream));

            this.contentID = contentID;

            contentPath = fullPath;
        }

        private void Close()
        {
            ContentStream.Close();
            ContentStream = null;

            // kui path on teada, siis on temp fail ja see tuleb kustutada ..
            if (!string.IsNullOrEmpty(contentPath) && File.Exists(contentPath))
                File.Delete(contentPath);
        }

        public string ToBase64String()
        {
            ContentStream.Position = 0;

            var buffer = new byte[ContentStream.Length];
            ContentStream.Read(buffer, 0, buffer.Length);

            return Convert.ToBase64String(buffer, Base64FormattingOptions.InsertLineBreaks);
        }

        public void WriteAsBase64(XmlWriter writer)
        {
            ContentStream.Position = 0;

            const int bufferSize = 1000;

            int bytesRead;
            var buffer = new byte[bufferSize];

            while ((bytesRead = ContentStream.Read(buffer, 0, bufferSize)) > 0)
                writer.WriteBase64(buffer, 0, bytesRead);
        }

        public void Dispose()
        {
            Close();
        }
    }
}