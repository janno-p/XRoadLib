#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace System.Web.Services.Description
{
    public class ServiceDescription : NamedItem
    {
        public List<Binding> Bindings { get; } = new List<Binding>();
        public List<Message> Messages { get; } = new List<Message>();
        public List<PortType> PortTypes { get; } = new List<PortType>();
        public List<Service> Services { get; } = new List<Service>();
        public string TargetNamespace { get; set; }
        public Types Types { get; } = new Types();

        public void Write(XmlWriter writer)
        {
            var serviceDescriptionWriter = new ServiceDescriptionWriter(writer);
            serviceDescriptionWriter.WriteServiceDescription(this);
        }
    }
}

#endif
