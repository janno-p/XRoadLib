using XRoadLib.Serialization;

namespace XRoadLib
{
    public interface IXRoadRequest<TResponse> { }
    public interface IXRoadRequest : IXRoadRequest<UnitResponse> { }
}