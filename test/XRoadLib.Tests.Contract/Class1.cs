using System.Collections.Generic;
using XRoadLib.Attributes;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    [XRoadSerializable]
    public class Class1 : ITrackSpecifiedMembers
    {
        IDictionary<string, bool> ITrackSpecifiedMembers.SpecifiedMembers { get; set; }
    }
}