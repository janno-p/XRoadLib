#if NETSTANDARD1_5

using System.Xml;

namespace System.Web.Services.Description
{
    public abstract class NamedItem : DocumentableItem
    {
        public string Name { get; set; }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);

            if (!string.IsNullOrWhiteSpace(Name))
                writer.WriteAttributeString("name", Name);
        }
    }
}

#endif