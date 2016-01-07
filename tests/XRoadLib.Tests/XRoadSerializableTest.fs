namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open XRoadLib.Serialization
open XRoadLib.Tests.Contract

[<TestFixture>]
module XRoadSerializableTest =
    let [<Literal>] memberName = "Member"
    let [<Literal>] noMemberName = "No Member"

    [<Test>]
    let ``adds member names after serialization`` () =
        let instance = Class1()

        instance.IsSpecified(memberName) |> should be False
        instance.IsAcceptedByTemplate(memberName) |> should be False
        instance.IsSpecified(noMemberName) |> should be False
        instance.IsAcceptedByTemplate(noMemberName) |> should be False

        let serializable = instance :> IXRoadSerializable
        serializable.OnMemberDeserialized(memberName)

        instance.IsSpecified(memberName) |> should be True
        instance.IsAcceptedByTemplate(memberName) |> should be True
        instance.IsSpecified(noMemberName) |> should be False
        instance.IsAcceptedByTemplate(noMemberName) |> should be False

        serializable.SetTemplateMembers([ memberName ])

        instance.IsSpecified(memberName) |> should be True
        instance.IsAcceptedByTemplate(memberName) |> should be True
        instance.IsSpecified(noMemberName) |> should be False
        instance.IsAcceptedByTemplate(noMemberName) |> should be False


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
