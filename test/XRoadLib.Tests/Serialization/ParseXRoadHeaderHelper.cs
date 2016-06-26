using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;
using XRoadLib.Protocols;
using XRoadLib.Protocols.Headers;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Serialization
{
    public static class ParseXRoadHeaderHelper
    {
        public static Tuple<IXRoadHeader, IList<XElement>, XRoadProtocol> ParseHeader(string xml, string ns)
        {
            using (var stream = new MemoryStream())
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
            {
                streamWriter.WriteLine(@"<Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:id=""http://x-road.eu/xsd/identifiers"" xmlns:repr=""http://x-road.eu/xsd/representation.xsd"">");
                streamWriter.WriteLine($"<Header xmlns:xrd=\"{ns}\">");
                streamWriter.WriteLine(xml);
                streamWriter.WriteLine(@"</Header>");
                streamWriter.WriteLine(@"</Envelope>");
                streamWriter.Flush();

                stream.Position = 0;
                using (var reader = new XRoadMessageReader(stream, new WebHeaderCollection(), Encoding.UTF8, Path.GetTempPath(), new[] { Globals.XRoadProtocol20, Globals.XRoadProtocol31, Globals.XRoadProtocol40 }))
                using (var msg = new XRoadMessage())
                {
                    reader.Read(msg, false);
                    return Tuple.Create(msg.Header, msg.UnresolvedHeaders, msg.Protocol);
                }
            }
        }
    }
}