#if NETSTANDARD1_5

namespace System.Web.Services.Description
{
    public class InputBinding : MessageBinding
    {
        protected override string ElementName { get; } = "input";
    }
}

#endif