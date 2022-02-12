using System.Runtime.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Schema;

[Serializable]
public class UnexpectedElementException : ContractViolationException
{
    [UsedImplicitly]
    public TypeDefinition TypeDefinition { get; }

    [UsedImplicitly]
    public ParticleDefinition ParticleDefinition { get; }

    [UsedImplicitly]
    public XName ElementName { get; }

    public UnexpectedElementException(string message, TypeDefinition typeDefinition, ParticleDefinition particleDefinition, XName elementName)
        : base(ClientFaultCode.UnexpectedElement, message)
    {
        TypeDefinition = typeDefinition;
        ParticleDefinition = particleDefinition;
        ElementName = elementName;
    }

    public UnexpectedElementException(TypeDefinition typeDefinition, ParticleDefinition particleDefinition, XName elementName)
        : this($"Expected element `{particleDefinition.Content.SerializedName.LocalName}` while deserializing type `{typeDefinition.Name}`, but element `{elementName}` was found instead.", typeDefinition, particleDefinition, elementName)
    { }

    protected UnexpectedElementException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}