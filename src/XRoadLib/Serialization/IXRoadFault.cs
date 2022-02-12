namespace XRoadLib.Serialization;

public interface IXRoadFault
{
    string FaultCode { get; }
    string FaultString { get; }
}