using System;
using System.Collections.Generic;
using System.Linq;

namespace XRoadLib.Serialization
{
    [Serializable]
    public abstract class XRoadSerializable : IXRoadSerializable
    {
        private readonly IDictionary<string, bool> initializedMembers = new Dictionary<string, bool>();

        public IDictionary<string, bool> GetInitializedMembers()
        {
            return initializedMembers;
        }

        public bool OnMärgitud(string väljaNimetus)
        {
            return initializedMembers.ContainsKey(väljaNimetus) && initializedMembers[väljaNimetus];
        }

        public bool OnValidaatoris(string väljaNimetus)
        {
            return initializedMembers.ContainsKey(väljaNimetus);
        }

        void IXRoadSerializable.OnMemberDeserialized(string memberName)
        {
            initializedMembers[memberName] = true;
        }

        void IXRoadSerializable.SetSpecifiedMembers(IEnumerable<string> memberNames)
        {
            foreach (var memberName in memberNames.Where(x => !initializedMembers.ContainsKey(x)))
                initializedMembers.Add(memberName, false);
        }
    }
}