using System;
using XRoadLib.Attributes;
using XRoadLib.Headers;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract.Operations
{
    [XRoadOperation]
    public class WithDateTimeOperation : XRoadOperation<DateTime, UnitResponse, XRoadHeader>
    { }
}