using XRoadLib.Attributes;
using XRoadLib.Headers;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract.Operations
{
    [XRoadOperation]
    public class WithBooleanOperation : XRoadOperation<bool, UnitResponse, XRoadHeader>
    { }
}