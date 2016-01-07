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

        public bool IsSpecified(string memberName)
        {
            bool value;
            return initializedMembers.TryGetValue(memberName, out value) && value;
        }

        public bool IsAcceptedByTemplate(string memberName)
        {
            return initializedMembers.ContainsKey(memberName);
        }

        void IXRoadSerializable.OnMemberDeserialized(string memberName)
        {
            initializedMembers[memberName] = true;
        }

        void IXRoadSerializable.SetTemplateMembers(IEnumerable<string> memberNames)
        {
            foreach (var memberName in memberNames.Where(x => !initializedMembers.ContainsKey(x)))
                initializedMembers.Add(memberName, false);
        }
    }
}