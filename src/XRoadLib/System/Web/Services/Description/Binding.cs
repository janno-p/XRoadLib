#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace System.Web.Services.Description
{
    public class Binding : NamedItem
    {
        public IList<OperationBinding> Operations { get; } = new List<OperationBinding>();
        public XmlQualifiedName Type { get; set; }
    }
}

#endif