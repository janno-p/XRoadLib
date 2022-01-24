using XRoadLib.Serialization;
using XRoadLib.Tests.Contract;

namespace XRoadLib.Tests.Serialization;

public class XRoadSerializableTest
{
    private const string MemberName = "Member";
    private const string NoMemberName = "No Member";

    [Fact]
    public void AddsMemberNamesAfterSerialization()
    {
        var instance = new Class1();

        Assert.False(instance.IsSpecified(MemberName));
        Assert.False(instance.IsAcceptedByTemplate(MemberName));
        Assert.False(instance.IsSpecified(NoMemberName));
        Assert.False(instance.IsAcceptedByTemplate(NoMemberName));

        ((IXRoadSerializable)instance).OnMemberDeserialized(MemberName);

        Assert.True(instance.IsSpecified(MemberName));
        Assert.True(instance.IsAcceptedByTemplate(MemberName));
        Assert.False(instance.IsSpecified(NoMemberName));
        Assert.False(instance.IsAcceptedByTemplate(NoMemberName));

        ((IXRoadSerializable)instance).SetTemplateMembers(new[] { MemberName });

        Assert.True(instance.IsSpecified(MemberName));
        Assert.True(instance.IsAcceptedByTemplate(MemberName));
        Assert.False(instance.IsSpecified(NoMemberName));
        Assert.False(instance.IsAcceptedByTemplate(NoMemberName));
    }

    [Fact]
    public void ExistsOnlyInTemplateWhenNotSerialized()
    {
        var instance = new Class1();

        Assert.False(instance.IsSpecified(MemberName));
        Assert.False(instance.IsAcceptedByTemplate(MemberName));

        ((IXRoadSerializable)instance).SetTemplateMembers(new [] { MemberName });

        Assert.False(instance.IsSpecified(MemberName));
        Assert.True(instance.IsAcceptedByTemplate(MemberName));

        ((IXRoadSerializable)instance).OnMemberDeserialized(MemberName);

        Assert.True(instance.IsSpecified(MemberName));
        Assert.True(instance.IsAcceptedByTemplate(MemberName));
    }

    [Fact]
    public void CanHandleMemberAddedMultipleTimes()
    {
        var instance = new Class1();

        Assert.False(instance.IsSpecified(MemberName));
        Assert.False(instance.IsAcceptedByTemplate(MemberName));

        ((IXRoadSerializable)instance).SetTemplateMembers(new [] { MemberName, "muu", "kolmas", MemberName });

        Assert.False(instance.IsSpecified(MemberName));
        Assert.True(instance.IsAcceptedByTemplate(MemberName));
    }
}