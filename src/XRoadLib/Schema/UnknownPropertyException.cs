using System.Runtime.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Schema;

[Serializable]
public class UnknownPropertyException : ContractViolationException
{
    [UsedImplicitly]
    public TypeDefinition TypeDefinition { get; }

    [UsedImplicitly]
    public XName PropertyName { get; }

    public UnknownPropertyException(string message, TypeDefinition typeDefinition, XName propertyName)
        : base(ClientFaultCode.UnknownProperty, message)
    {
        TypeDefinition = typeDefinition;
        PropertyName = propertyName;
    }

    [UsedImplicitly]
    public UnknownPropertyException(TypeDefinition typeDefinition, XName propertyName)
        : this($"Type `{typeDefinition.Name}` does not define property `{propertyName}` (property names are case-sensitive).", typeDefinition, propertyName)
    { }

    protected UnknownPropertyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}