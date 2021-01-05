using System.Diagnostics.CodeAnalysis;
using XRoadLib.Soap;

namespace XRoadLib.Schema
{
    public class UnsupportedContentTransferEncodingException : ContractViolationException
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public string ContentTransferEncoding { get; }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public UnsupportedContentTransferEncodingException(string message, string contentTransferEncoding)
            : base(ClientFaultCode.UnsupportedContentTransferEncoding, message)
        {
            ContentTransferEncoding = contentTransferEncoding;
        }

        public UnsupportedContentTransferEncodingException(string contentTransferEncoding)
            : this($"Content transfer encoding `{contentTransferEncoding}` is not supported by the adapter.", contentTransferEncoding)
        { }
    }
}