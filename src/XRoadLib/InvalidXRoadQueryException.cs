using XRoadLib.Soap;

namespace XRoadLib
{
    /// <summary>
    /// Describes everything that is wrong with current X-Road request message.
    /// </summary>
    public class InvalidXRoadQueryException : XRoadException
    {
        public InvalidXRoadQueryException(string message)
            : base(new ClientFaultCode("InvalidQuery"), message)
        { }
    }
}