using XRoadLib.Attributes;

namespace XRoadLib.Tests.Contract
{
    public interface IService
    {
        [XRoadService("Service1")]
        int Service1(Service1Request request);
    }
}