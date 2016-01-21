using XRoadLib.Attributes;

namespace XRoadLib.Tests.Contract
{
    public interface IService
    {
        [XRoadService("Service1")]
        int Service1(
            ParamType1 param1,
            ParamType2 param2,
            ParamType3 param3
            );
    }
}