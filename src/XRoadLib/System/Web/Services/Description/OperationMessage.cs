#if NETSTANDARD1_5

using System.Xml;

namespace System.Web.Services.Description
{
    public class OperationMessage : NamedItem
    {
        public XmlQualifiedName Message { get; set; }
    }
}

#endif