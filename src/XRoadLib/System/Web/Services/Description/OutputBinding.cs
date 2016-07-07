#if NETSTANDARD1_5

namespace System.Web.Services.Description
{
    public class OutputBinding : MessageBinding
    {
        protected override string ElementName { get; } = "output";
    }
}

#endif