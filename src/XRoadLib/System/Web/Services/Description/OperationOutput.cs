#if NETSTANDARD1_6_1

namespace System.Web.Services.Description
{
    public class OperationOutput : OperationMessage
    {
        protected override string ElementName { get; } = "output";
    }
}

#endif