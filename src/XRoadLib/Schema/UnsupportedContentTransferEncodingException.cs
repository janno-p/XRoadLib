using XRoadLib.Soap;

namespace XRoadLib.Schema
{
    public class UnsupportedContentTransferEncodingException : ContractViolationException
    {
        public string ContentTransferEncoding { get; }

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