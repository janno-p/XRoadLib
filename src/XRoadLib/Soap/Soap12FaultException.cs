using System.Runtime.Serialization;

namespace XRoadLib.Soap;

[Serializable]
public class Soap12FaultException : Exception
{
    [UsedImplicitly]
    public ISoap12Fault Fault { get; }

    public Soap12FaultException(ISoap12Fault fault)
        : base(fault?.Reason.First().Text)
    {
        Fault = fault ?? throw new ArgumentNullException(nameof(fault));
    }

    protected Soap12FaultException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}