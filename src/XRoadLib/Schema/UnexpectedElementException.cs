using XRoadLib.Soap;

namespace XRoadLib.Schema
{
    public class UnexpectedElementException : ContractViolationException
    {
        public TypeDefinition TypeDefinition { get; }
        public ParticleDefinition ParticleDefinition { get; }
        public string ElementName { get; }

        public UnexpectedElementException(string message, TypeDefinition typeDefinition, ParticleDefinition particleDefinition, string elementName)
            : base(ClientFaultCode.UnexpectedElement, message)
        {
            TypeDefinition = typeDefinition;
            ParticleDefinition = particleDefinition;
            ElementName = elementName;
        }

        public UnexpectedElementException(TypeDefinition typeDefinition, ParticleDefinition particleDefinition, string elementName)
            : this($"Expected element `{particleDefinition.Content.SerializedName.LocalName}` while deserializing type `{typeDefinition.Name}`, but element `{elementName}` was found instead.", typeDefinition, particleDefinition, elementName)
        { }
    }
}