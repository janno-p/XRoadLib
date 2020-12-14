using XRoadLib.Extensions;

using XRoadLib.Tests.Contract;
using Xunit;

namespace XRoadLib.Tests.Serialization
{
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

            instance.AddSpecifiedMember(MemberName);

            Assert.True(instance.IsSpecified(MemberName));
            Assert.True(instance.IsAcceptedByTemplate(MemberName));
            Assert.False(instance.IsSpecified(NoMemberName));
            Assert.False(instance.IsAcceptedByTemplate(NoMemberName));

            instance.AddTemplateMember(MemberName);

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

            instance.AddTemplateMember(MemberName);

            Assert.False(instance.IsSpecified(MemberName));
            Assert.True(instance.IsAcceptedByTemplate(MemberName));

            instance.AddSpecifiedMember(MemberName);

            Assert.True(instance.IsSpecified(MemberName));
            Assert.True(instance.IsAcceptedByTemplate(MemberName));
        }

        [Fact]
        public void CanHandleMemberAddedMultipleTimes()
        {
            var instance = new Class1();

            Assert.False(instance.IsSpecified(MemberName));
            Assert.False(instance.IsAcceptedByTemplate(MemberName));

            instance.AddTemplateMember(MemberName)
                    .AddTemplateMember("muu")
                    .AddTemplateMember("kolmas")
                    .AddTemplateMember(MemberName);

            Assert.False(instance.IsSpecified(MemberName));
            Assert.True(instance.IsAcceptedByTemplate(MemberName));
        }
    }
}
