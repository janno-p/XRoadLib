using System.IO;
using System.Xml;
using XRoadLib.Wsdl;

namespace XRoadLib.Extensions
{
    public static class ServiceDescriptionExtensions
    {
        /// <summary>
        /// Outputs service description to specified stream.
        /// </summary>
        public static void SaveTo(this ServiceDescription serviceDescription, Stream stream)
        {
            using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = "\r\n" }))
            {
                writer.WriteStartDocument();
                serviceDescription.Write(writer);
                writer.WriteEndDocument();
                writer.Flush();
            }
        }
    }
}