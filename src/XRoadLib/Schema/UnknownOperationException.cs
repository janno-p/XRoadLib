using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using XRoadLib.Soap;

namespace XRoadLib.Schema
{
    public class UnknownOperationException : ContractViolationException
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public XName QualifiedName { get; }

        public UnknownOperationException(string message, XName qualifiedName)
            : base(ClientFaultCode.UnknownOperation, message)
        {
            QualifiedName = qualifiedName;
        }

        public UnknownOperationException(XName qualifiedName)
            : this($"The operation `{qualifiedName}` is not defined by contract.", qualifiedName)
        { }
    }
}