namespace XRoadLib.Serialization;

public interface IXRoadSerializable
{
    void OnMemberDeserialized(string memberName);

    void SetTemplateMembers(IEnumerable<string> memberNames);

    bool IsSpecified(string memberName);
}