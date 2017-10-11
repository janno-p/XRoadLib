#if NETSTANDARD1_6

using System.Collections.Generic;
using System.Xml;

namespace System.Web.Services.Description
{
    public class ServiceDescription : NamedItem
    {
        protected override string ElementName { get; } = "definitions";

        public List<Binding> Bindings { get; } = new List<Binding>();
        public List<Message> Messages { get; } = new List<Message>();
        public List<PortType> PortTypes { get; } = new List<PortType>();
        public List<Service> Services { get; } = new List<Service>();
        public string TargetNamespace { get; set; }
        public Types Types { get; } = new Types();

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);

            if (!string.IsNullOrWhiteSpace(TargetNamespace))
                writer.WriteAttributeString("targetNamespace", TargetNamespace);
        }

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            //Imports.ForEach(x => x.Write(writer));
            Types?.Write(writer);
            Messages.ForEach(x => x.Write(writer));
            PortTypes.ForEach(x => x.Write(writer));
            Bindings.ForEach(x => x.Write(writer));
            Services.ForEach(x => x.Write(writer));
        }
    }
}

#endif
