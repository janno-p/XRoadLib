using System.Xml.Linq;
using XRoadLib.Soap;

namespace XRoadLib.Schema
{
    public class UnknownTypeException : ContractViolationException
    {
        public ParticleDefinition ParticleDefinition { get; }
        public TypeDefinition TypeDefinition { get; }
        public XName QualifiedName { get; }

        public UnknownTypeException(string message, ParticleDefinition particleDefinition, TypeDefinition typeDefinition, XName qualifiedName)
            : base(ClientFaultCode.UnknownType, message)
        {
            ParticleDefinition = particleDefinition;
            TypeDefinition = typeDefinition;
            QualifiedName = qualifiedName;
        }

        public UnknownTypeException(ParticleDefinition particleDefinition, TypeDefinition typeDefinition, XName qualifiedName)
            : this($"The referenced type `{qualifiedName}` is not defined by contract.", particleDefinition, typeDefinition, qualifiedName)
        { }
    }
}