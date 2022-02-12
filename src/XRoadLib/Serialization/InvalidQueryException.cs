using System.Runtime.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Serialization;

/// <summary>
/// Describes everything that is wrong with current X-Road request message.
/// </summary>
[Serializable]
public class InvalidQueryException : XRoadException
{
    public InvalidQueryException(string message)
        : base(ClientFaultCode.InvalidQuery, message)
    { }

    protected InvalidQueryException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}