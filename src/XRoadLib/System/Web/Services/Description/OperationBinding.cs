#if NETSTANDARD1_5

namespace System.Web.Services.Description
{
    public class OperationBinding : NamedItem
    {
        public InputBinding Input { get; set; }
        public OutputBinding Output { get; set; }
    }
}

#endif