using System.Runtime.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Schema;

[Serializable]
public class UnknownOperationException : ContractViolationException
{
    [UsedImplicitly]
    public XName QualifiedName { get; }

    public UnknownOperationException(string message, XName qualifiedName)
        : base(ClientFaultCode.UnknownOperation, message)
    {
        QualifiedName = qualifiedName;
    }

    public UnknownOperationException(XName qualifiedName)
        : this($"The operation `{qualifiedName}` is not defined by contract.", qualifiedName)
    { }

    protected UnknownOperationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}