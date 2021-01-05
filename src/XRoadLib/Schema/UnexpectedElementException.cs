using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using XRoadLib.Soap;

namespace XRoadLib.Schema
{
    public class UnexpectedElementException : ContractViolationException
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public TypeDefinition TypeDefinition { get; }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public ParticleDefinition ParticleDefinition { get; }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
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
    }
}