using System.Collections.Generic;

namespace XRoadLib.Serialization
{
    public interface IXRoadSerializable
    {
        void OnMemberDeserialized(string memberName);

        void SetSpecifiedMembers(IEnumerable<string> memberNames);
    }
}