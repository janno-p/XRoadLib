#if NETSTANDARD1_5

using System.Collections.Generic;

namespace System.Web.Services.Description
{
    public class Service : NamedItem
    {
        public IList<Port> Ports { get; } = new List<Port>();
    }
}

#endif