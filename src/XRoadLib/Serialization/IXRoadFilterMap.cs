using System.Collections.Generic;

namespace XRoadLib.Serialization
{
    public interface IXRoadFilterMap
    {
        string GroupName { get; }

        ISet<string> EnabledProperties { get; }
    }
}