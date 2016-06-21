#if NETSTANDARD1_5

using System.Collections.Generic;

namespace System.Web.Services.Description
{
    public class DocumentableItem
    {
        public IDictionary<string, string> Namespaces { get; } = new Dictionary<string, string>();
    }
}

#endif