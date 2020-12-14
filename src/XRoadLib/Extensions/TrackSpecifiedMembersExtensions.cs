using System.Collections.Generic;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions
{
    public static class TrackSpecifiedMembersExtensions
    {
        public static bool IsSpecified(this ITrackSpecifiedMembers dtoObject, string memberName) =>
            dtoObject.SpecifiedMembers != null && dtoObject.SpecifiedMembers.TryGetValue(memberName, out var value) && value;

        public static bool IsAcceptedByTemplate(this ITrackSpecifiedMembers dtoObject, string memberName) =>
            dtoObject.SpecifiedMembers != null && dtoObject.SpecifiedMembers.ContainsKey(memberName);

        public static ITrackSpecifiedMembers AddSpecifiedMember(this ITrackSpecifiedMembers dtoObject, string memberName)
        {
            dtoObject.SpecifiedMembers ??= new Dictionary<string, bool>();
            dtoObject.SpecifiedMembers[memberName] = true;
            return dtoObject;
        }

        public static ITrackSpecifiedMembers AddTemplateMember(this ITrackSpecifiedMembers dtoObject, string memberName)
        {
            dtoObject.SpecifiedMembers ??= new Dictionary<string, bool>();
            if (!dtoObject.SpecifiedMembers.ContainsKey(memberName))
                dtoObject.SpecifiedMembers[memberName] = false;
            return dtoObject;
        }

        internal static object SetSpecifiedMembers(this object dtoObject, IDictionary<string, bool> specifiedMembers)
        {
            if (dtoObject is ITrackSpecifiedMembers trackSpecifiedMembers)
                trackSpecifiedMembers.SpecifiedMembers = specifiedMembers;

            return dtoObject;
        }
    }
}