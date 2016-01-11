namespace XRoadLib.Attributes
{
    public interface IXRoadProtocolAppliable
    {
        bool HasAppliesToValue { get; }

        XRoadProtocol AppliesTo { get; set; }
    }
}