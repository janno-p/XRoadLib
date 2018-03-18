using XRoadLib.Soap;

namespace XRoadLib
{
    /// <summary>
    /// Describes everything that is wrong with current X-Road request message.
    /// </summary>
    public class InvalidQueryException : XRoadException
    {
        public InvalidQueryException(string message)
            : base(ClientFaultCode.InvalidQuery, message)
        { }
    }
}