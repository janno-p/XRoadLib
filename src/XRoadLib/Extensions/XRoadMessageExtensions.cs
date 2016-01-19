using System.Xml;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions
{
    public static class XRoadMessageExtensions
    {
        public static IXRoadFault DeserializeXRoadFault(this XRoadMessage message)
        {
            message.ContentStream.Position = 0;
            using (var reader = XmlReader.Create(message.ContentStream))
            {
                reader.MoveToPayload(message.RootElementName);

                var responseElementName = message.Protocol.GetResponseElementName();
                if (!reader.MoveToElement(3, responseElementName.LocalName))
                    throw XRoadException.InvalidQuery("X-Road fault should be wrapped inside `{0}` element.", responseElementName);

                var fault = new XRoadFault();

                while (reader.Read() && reader.MoveToElement(4))
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    if (string.IsNullOrWhiteSpace(reader.NamespaceURI) && reader.LocalName == "faultCode")
                        fault.FaultCode = reader.ReadElementContentAsString();

                    if (string.IsNullOrWhiteSpace(reader.NamespaceURI) && reader.LocalName == "faultString")
                        fault.FaultString = reader.ReadElementContentAsString();
                }

                return fault;
            }
        }
    }
}