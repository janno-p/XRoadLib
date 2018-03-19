using XRoadLib.Soap;

namespace XRoadLib.Schema
{
    public class UnknownPropertyException : ContractViolationException
    {
        public TypeDefinition TypeDefinition { get; }
        public string PropertyName { get; }

        public UnknownPropertyException(string message, TypeDefinition typeDefinition, string propertyName)
            : base(ClientFaultCode.UnknownProperty, message)
        {
            TypeDefinition = typeDefinition;
            PropertyName = propertyName;
        }

        public UnknownPropertyException(TypeDefinition typeDefinition, string propertyName)
            : this($"Type `{typeDefinition.Name}` does not define property `{propertyName}` (property names are case-sensitive).", typeDefinition, propertyName)
        { }
    }
}