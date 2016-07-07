#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace System.Web.Services.Description
{
    public class Operation : NamedItem
    {
        protected override string ElementName { get; } = "operation";

        public List<OperationMessage> Messages { get; } = new List<OperationMessage>();

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            Messages.ForEach(x => x.Write(writer));
        }
    }
}

#endif