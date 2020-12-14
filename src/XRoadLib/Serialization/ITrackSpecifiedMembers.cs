using System.Collections.Generic;

namespace XRoadLib.Serialization
{
    public interface ITrackSpecifiedMembers
    {
        IDictionary<string, bool> SpecifiedMembers { get; set; }
    }
}