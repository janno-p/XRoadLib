using System.Runtime.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Schema;

[Serializable]
public class UnexpectedValueException : ContractViolationException
{
    public TypeDefinition TypeDefinition { get; }
    public object Value { get; }

    public UnexpectedValueException(string message, TypeDefinition typeDefinition, object value)
        : base(ClientFaultCode.UnexpectedValue, message)
    {
        TypeDefinition = typeDefinition;
        Value = value;
    }

    protected UnexpectedValueException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}