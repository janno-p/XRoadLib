#if NETSTANDARD1_5

using System.Collections.Generic;

namespace System.Web.Services.Description
{
    public class Operation : NamedItem
    {
        public List<OperationMessage> Messages { get; } = new List<OperationMessage>();
    }
}

#endif