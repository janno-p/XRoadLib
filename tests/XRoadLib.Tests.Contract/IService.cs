using XRoadLib.Attributes;

namespace XRoadLib.Tests.Contract
{
    public interface IService
    {
        [XRoadService("Service1")]
        int Service1(
            [XRoadParameter(IsOptional = true)] ParamType1 param1,
            [XRoadParameter(IsOptional = true)] ParamType2 param2,
            [XRoadParameter(IsOptional = true)] ParamType3 param3
            );
    }
}