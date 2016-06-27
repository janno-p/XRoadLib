#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;
using XRoadLib;

namespace System.Web.Services.Description
{
    public class ServiceDescription : NamedItem
    {
        public IList<Binding> Bindings { get; } = new List<Binding>();
        public IList<Message> Messages { get; } = new List<Message>();
        public IList<PortType> PortTypes { get; } = new List<PortType>();
        public IList<Service> Services { get; } = new List<Service>();
        public string TargetNamespace { get; set; }
        public Types Types { get; } = new Types();

        public void Write(XmlWriter writer)
        {
            writer.WriteStartElement("definitions", NamespaceConstants.WSDL);

            writer.WriteEndElement();
        }
    }
}

#endif
