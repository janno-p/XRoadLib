#if NETSTANDARD2_0

namespace System.Web.Services.Description
{
    public class OperationOutput : OperationMessage
    {
        protected override string ElementName { get; } = "output";
    }
}

#endif