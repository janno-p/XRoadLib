#if NETSTANDARD1_5

using System.Collections.Generic;

namespace System.Web.Services.Description
{
    public class PortType : NamedItem
    {
        public List<Operation> Operations { get; } = new List<Operation>();
    }
}

#endif