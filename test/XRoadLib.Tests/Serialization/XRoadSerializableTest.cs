using XRoadLib.Serialization;
using XRoadLib.Tests.Contract;
using Xunit;

namespace XRoadLib.Tests.Serialization
{
    public class XRoadSerializableTest
    {
        private const string MEMBER_NAME = "Member";
        private const string NO_MEMBER_NAME = "No Member";

        [Fact]
        public void AddsMemberNamesAfterSerialization()
        {
            var instance = new Class1();

            Assert.False(instance.IsSpecified(MEMBER_NAME));
            Assert.False(instance.IsAcceptedByTemplate(MEMBER_NAME));
            Assert.False(instance.IsSpecified(NO_MEMBER_NAME));
            Assert.False(instance.IsAcceptedByTemplate(NO_MEMBER_NAME));

            ((IXRoadSerializable)instance).OnMemberDeserialized(MEMBER_NAME);

            Assert.True(instance.IsSpecified(MEMBER_NAME));
            Assert.True(instance.IsAcceptedByTemplate(MEMBER_NAME));
            Assert.False(instance.IsSpecified(NO_MEMBER_NAME));
            Assert.False(instance.IsAcceptedByTemplate(NO_MEMBER_NAME));

            ((IXRoadSerializable)instance).SetTemplateMembers(new[] { MEMBER_NAME });

            Assert.True(instance.IsSpecified(MEMBER_NAME));
            Assert.True(instance.IsAcceptedByTemplate(MEMBER_NAME));
            Assert.False(instance.IsSpecified(NO_MEMBER_NAME));
            Assert.False(instance.IsAcceptedByTemplate(NO_MEMBER_NAME));
        }

        [Fact]
        public void ExistsOnlyInTemplateWhenNotSerialized()
        {
            var instance = new Class1();

            Assert.False(instance.IsSpecified(MEMBER_NAME));
            Assert.False(instance.IsAcceptedByTemplate(MEMBER_NAME));

            ((IXRoadSerializable)instance).SetTemplateMembers(new [] { MEMBER_NAME });

            Assert.False(instance.IsSpecified(MEMBER_NAME));
            Assert.True(instance.IsAcceptedByTemplate(MEMBER_NAME));

            ((IXRoadSerializable)instance).OnMemberDeserialized(MEMBER_NAME);

            Assert.True(instance.IsSpecified(MEMBER_NAME));
            Assert.True(instance.IsAcceptedByTemplate(MEMBER_NAME));
        }

        [Fact]
        public void CanHandleMemberAddedMultipleTimes()
        {
            var instance = new Class1();

            Assert.False(instance.IsSpecified(MEMBER_NAME));
            Assert.False(instance.IsAcceptedByTemplate(MEMBER_NAME));

            ((IXRoadSerializable)instance).SetTemplateMembers(new [] { MEMBER_NAME, "muu", "kolmas", MEMBER_NAME });

            Assert.False(instance.IsSpecified(MEMBER_NAME));
            Assert.True(instance.IsAcceptedByTemplate(MEMBER_NAME));
        }
    }
}
