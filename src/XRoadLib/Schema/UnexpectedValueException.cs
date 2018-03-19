using XRoadLib.Soap;

namespace XRoadLib.Schema
{
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
    }
}