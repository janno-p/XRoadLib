using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using XRoadLib.Headers;
using XRoadLib.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Tests.Serialization
{
    public static class ParseXRoadHeaderHelper
    {
        private static readonly IMessageFormatter MessageFormatter = new SoapMessageFormatter();

        public static async Task<Tuple<ISoapHeader, IList<XElement>, IServiceManager>> ParseHeaderAsync(string xml, string ns)
        {
            using var stream = new MemoryStream();
            using var streamWriter = new StreamWriter(stream, XRoadEncoding.Utf8);

            await streamWriter.WriteLineAsync(@"<Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:id=""http://x-road.eu/xsd/identifiers"" xmlns:repr=""http://x-road.eu/xsd/representation.xsd"">");
            await streamWriter.WriteLineAsync($"<Header xmlns:xrd=\"{ns}\">");
            await streamWriter.WriteLineAsync(xml);
            await streamWriter.WriteLineAsync(@"</Header>");
            await streamWriter.WriteLineAsync(@"</Envelope>");
            await streamWriter.FlushAsync();

            stream.Position = 0;

            using var reader = new XRoadMessageReader(new DataReader(stream), MessageFormatter, "text/xml; charset=UTF-8", Globals.StoragePath, new IServiceManager[] { Globals.ServiceManager });
            using var msg = new XRoadMessage();

            await reader.ReadAsync(msg);

            return Tuple.Create(msg.Header, msg.UnresolvedHeaders, msg.ServiceManager);
        }
    }
}