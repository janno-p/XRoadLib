using XRoadLib.Attributes;
using XRoadLib.Headers;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract.Operations
{
    [XRoadOperation]
    public class WithBinaryOperation : XRoadOperation<XRoadBinaryTestDto, UnitResponse, XRoadHeader>
    { }
}