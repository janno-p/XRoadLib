using XRoadLib.Attributes;
using XRoadLib.Headers;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract.Operations
{
    [XRoadOperation]
    public class WithIntOperation : XRoadOperation<int, UnitResponse, XRoadHeader>
    { }
}