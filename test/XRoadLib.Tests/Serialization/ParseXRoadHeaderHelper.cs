using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using XRoadLib.Headers;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Serialization
{
    public static class ParseXRoadHeaderHelper
    {
        public static Tuple<ISoapHeader, IList<XElement>, IServiceManager> ParseHeader(string xml, string ns)
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
                using (var reader = new XRoadMessageReader(stream, "text/xml; charset=UTF-8", Path.GetTempPath(), new IServiceManager[] { Globals.ServiceManager20, Globals.ServiceManager31, Globals.ServiceManager40 }))
                using (var msg = new XRoadMessage())
                {
                    reader.Read(msg);
                    return Tuple.Create(msg.Header, msg.UnresolvedHeaders, msg.ServiceManager);
                }
            }
        }
    }
}