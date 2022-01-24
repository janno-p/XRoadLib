using System.Xml;
using System.Xml.Linq;

namespace XRoadLib.Tests;

public static class XDocumentExtensions
{
    public static XmlReader CreateAsyncReader(this XDocument document) =>
        XmlReader.Create(new StringReader(document.ToString()), new XmlReaderSettings { Async = true });
}