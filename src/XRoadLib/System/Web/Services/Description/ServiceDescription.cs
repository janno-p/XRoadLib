#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace System.Web.Services.Description
{
    public class ServiceDescription : NamedItem
    {
        public IList<Binding> Bindings { get; } = new List<Binding>();
        public IList<Message> Messages { get; } = new List<Message>();
        public IList<PortType> PortTypes { get; } = new List<PortType>();
        public IList<Service> Services { get; } = new List<Service>();
        public string TargetNamespace { get; set; }
        public Types Types { get; set; }

        public void Write(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}

#endif
