#if NETSTANDARD1_6

using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public abstract class XmlSchemaObject
    {
        protected abstract string ElementName { get; }

        public Dictionary<string, string> Namespaces { get; } = new Dictionary<string, string>();

        internal void Write(XmlWriter writer)
        {
            WriteStartElement(writer, ElementName);
            WriteAttributes(writer);
            WriteElements(writer);
            writer.WriteEndElement();
        }

        protected virtual void WriteElements(XmlWriter writer)
        {

        }

        protected virtual void WriteAttributes(XmlWriter writer)
        {
            Namespaces.Where(x => !string.IsNullOrWhiteSpace(x.Value) && writer.LookupPrefix(x.Value) != x.Key)
                      .ToList()
                      .ForEach(ns => writer.WriteAttributeString("xmlns", ns.Key, NamespaceConstants.XMLNS, ns.Value));
        }

        private void WriteStartElement(XmlWriter writer, string name)
        {
            var prefix = Namespaces.Where(kvp => kvp.Value == NamespaceConstants.WSDL)
                                   .Select(kvp => kvp.Key).SingleOrDefault() ?? writer.LookupPrefix(NamespaceConstants.XSD);
            writer.WriteStartElement(prefix, name, NamespaceConstants.XSD);
        }
    }
}

#endif
