namespace XRoadLib.Header
{
    public interface IXRoadHeader
    {
        XRoadClientIdentifier Client { get; }
        XRoadServiceIdentifier Service { get; }

        string UserId { get; }
        string Id { get; }
        string Issue { get; }
        string ProtocolVersion { get; }
    }
}