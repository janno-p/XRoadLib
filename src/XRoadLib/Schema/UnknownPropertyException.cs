using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using XRoadLib.Soap;

namespace XRoadLib.Schema
{
    public class UnknownPropertyException : ContractViolationException
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public TypeDefinition TypeDefinition { get; }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public XName PropertyName { get; }

        public UnknownPropertyException(string message, TypeDefinition typeDefinition, XName propertyName)
            : base(ClientFaultCode.UnknownProperty, message)
        {
            TypeDefinition = typeDefinition;
            PropertyName = propertyName;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public UnknownPropertyException(TypeDefinition typeDefinition, XName propertyName)
            : this($"Type `{typeDefinition.Name}` does not define property `{propertyName}` (property names are case-sensitive).", typeDefinition, propertyName)
        { }
    }
}