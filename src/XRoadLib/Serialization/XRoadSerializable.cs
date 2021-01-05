using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace XRoadLib.Serialization
{
    public abstract class XRoadSerializable : IXRoadSerializable
    {
        private readonly IDictionary<string, bool> _initializedMembers = new Dictionary<string, bool>();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public IDictionary<string, bool> GetInitializedMembers()
        {
            return _initializedMembers;
        }

        public bool IsSpecified(string memberName)
        {
            return _initializedMembers.TryGetValue(memberName, out var value) && value;
        }

        public bool IsAcceptedByTemplate(string memberName)
        {
            return _initializedMembers.ContainsKey(memberName);
        }

        void IXRoadSerializable.OnMemberDeserialized(string memberName)
        {
            _initializedMembers[memberName] = true;
        }

        void IXRoadSerializable.SetTemplateMembers(IEnumerable<string> memberNames)
        {
            foreach (var memberName in memberNames.Where(x => !_initializedMembers.ContainsKey(x)))
                _initializedMembers.Add(memberName, false);
        }
    }
}