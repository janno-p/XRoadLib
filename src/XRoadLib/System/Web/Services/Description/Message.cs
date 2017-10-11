#if NETSTANDARD1_6

using System.Collections.Generic;
using System.Xml;

namespace System.Web.Services.Description
{
    public class Message : NamedItem
    {
        protected override string ElementName { get; } = "message";

        public List<MessagePart> Parts { get; } = new List<MessagePart>();

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            Parts.ForEach(x => x.Write(writer));
        }
    }
}

#endif