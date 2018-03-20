using System.Xml.Linq;
using XRoadLib.Soap;

namespace XRoadLib.Schema
{
    public class UnknownOperationException : ContractViolationException
    {
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