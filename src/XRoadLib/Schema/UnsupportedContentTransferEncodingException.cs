using System.Runtime.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Schema;

[Serializable]
public class UnsupportedContentTransferEncodingException : ContractViolationException
{
    [UsedImplicitly]
    public string ContentTransferEncoding { get; }

    [UsedImplicitly]
    public UnsupportedContentTransferEncodingException(string message, string contentTransferEncoding)
        : base(ClientFaultCode.UnsupportedContentTransferEncoding, message)
    {
        ContentTransferEncoding = contentTransferEncoding;
    }

    public UnsupportedContentTransferEncodingException(string contentTransferEncoding)
        : this($"Content transfer encoding `{contentTransferEncoding}` is not supported by the adapter.", contentTransferEncoding)
    { }

    protected UnsupportedContentTransferEncodingException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        ContentTransferEncoding = default!;
    }
}