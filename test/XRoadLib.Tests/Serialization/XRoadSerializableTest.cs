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
    }

/*
open FsUnit
open NUnit.Framework
open XRoadLib.Serialization
open XRoadLib.Tests.Contract

[<TestFixture>]
module XRoadSerializableTest =





    [<Test>]
    let ``only in template when not serialized`` () =
        let instance = Class1()

        instance.IsSpecified(memberName) |> should be False
        instance.IsAcceptedByTemplate(memberName) |> should be False

        let serializable = instance :> IXRoadSerializable
        serializable.SetTemplateMembers([ memberName ])

        instance.IsSpecified(memberName) |> should be False
        instance.IsAcceptedByTemplate(memberName) |> should be True

        serializable.OnMemberDeserialized(memberName)

        instance.IsSpecified(memberName) |> should be True
        instance.IsAcceptedByTemplate(memberName) |> should be True

    [<Test>]
    let ``can handle member added multiple times`` () =
        let instance = Class1()

        instance.IsSpecified(memberName) |> should be False
        instance.IsAcceptedByTemplate(memberName) |> should be False

        let serializable = instance :> IXRoadSerializable
        serializable.SetTemplateMembers([ memberName; "muu"; "kolmas"; memberName ])

        instance.IsSpecified(memberName) |> should be False
        instance.IsAcceptedByTemplate(memberName) |> should be True
*/
}
