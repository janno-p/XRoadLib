#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public abstract class XmlSchemaObject
    {
        public Dictionary<string, string> Namespaces { get; } = new Dictionary<string, string>();

        internal abstract void Write(XmlWriter writer);

        protected virtual void WriteAttributes(XmlWriter writer)
        {
            Namespaces.Where(x => !string.IsNullOrWhiteSpace(x.Value) && writer.LookupPrefix(x.Value) != x.Key)
                      .ToList()
                      .ForEach(ns => writer.WriteAttributeString("xmlns", ns.Key, NamespaceConstants.XMLNS, ns.Value));
        }

        protected void WriteStartElement(XmlWriter writer, string name)
        {
            var prefix = Namespaces.Where(kvp => kvp.Value == NamespaceConstants.WSDL)
                                   .Select(kvp => kvp.Key).SingleOrDefault() ?? writer.LookupPrefix(NamespaceConstants.XSD);
            writer.WriteStartElement(prefix, name, NamespaceConstants.XSD);
        }
    }
}

#endif
