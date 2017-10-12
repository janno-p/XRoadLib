#if NETSTANDARD2_0

namespace System.Web.Services.Description
{
    public class OperationInput : OperationMessage
    {
        protected override string ElementName { get; } = "input";
    }
}

#endif