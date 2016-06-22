namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework;
open System
open System.IO
open System.Xml
open XRoadLib
open XRoadLib.Schema
open XRoadLib.Serialization
open XRoadLib.Serialization.Mapping

[<TestFixture>]
module DateTimeTest =
    let schemaDefinitionReader = SchemaDefinitionReader("")
    let dateTimeTypeMap = DateTimeTypeMap(schemaDefinitionReader.GetSimpleTypeDefinition<DateTime>("dateTime"))
    let deserializeValue x = dateTimeTypeMap |> deserializeValue x

    [<Test>]
    let ``can deserialize date part only`` () =
        let instance = deserializeValue "2013-08-27"
        instance |> should not' (be Null)
        let dateTime: DateTime = unbox instance
        dateTime.Kind |> should equal DateTimeKind.Unspecified
        dateTime.Year |> should equal 2013
        dateTime.Month |> should equal 8
        dateTime.Day |> should equal 27
        dateTime.Hour |> should equal 0
        dateTime.Minute |> should equal 0
        dateTime.Second |> should equal 0
        dateTime.Millisecond |> should equal 0

    [<Test>]
    let ``cannot deserialize wrong dateTime format`` () =
        TestDelegate(fun _ -> deserializeValue "2013-08-27T12:34:61" |> ignore)
        |> should (throwWithMessage @"The string '2013-08-27T12:34:61' is not a valid AllXsd value.") typeof<FormatException>

    [<Test>]
    let ``deserializes MinValue as null`` () =
        deserializeValue "0001-01-01T00:00:00" |> should be Null

    [<Test>]
    let ``can deserialize with time`` () =
        let instance = deserializeValue "2013-08-27T12:34:56"
        instance |> should not' (be Null)
        let dateTime: DateTime = unbox instance
        dateTime.Kind |> should equal DateTimeKind.Unspecified
        dateTime.Year |> should equal 2013
        dateTime.Month |> should equal 8
        dateTime.Day |> should equal 27
        dateTime.Hour |> should equal 12
        dateTime.Minute |> should equal 34
        dateTime.Second |> should equal 56
        dateTime.Millisecond |> should equal 0

    [<Test>]
    let ``deserialization ignores time zone value`` () =
        let instance = deserializeValue "2013-08-27T00:00:00+03:00"
        instance |> should not' (be Null)
        let dateTime: DateTime = unbox instance
        dateTime.Kind |> should equal DateTimeKind.Unspecified
        dateTime.Year |> should equal 2013
        dateTime.Month |> should equal 8
        dateTime.Day |> should equal 27
        dateTime.Hour |> should equal 0
        dateTime.Minute |> should equal 0
        dateTime.Second |> should equal 0
        dateTime.Millisecond |> should equal 0

    [<Test>]
    let ``deserialization ignores millisecond value`` () =
        let instance = deserializeValue "2013-08-27T12:34:56.1234+03:00"
        instance |> should not' (be Null)
        let dateTime: DateTime = unbox instance
        dateTime.Kind |> should equal DateTimeKind.Unspecified
        dateTime.Year |> should equal 2013
        dateTime.Month |> should equal 8
        dateTime.Day |> should equal 27
        dateTime.Hour |> should equal 12
        dateTime.Minute |> should equal 34
        dateTime.Second |> should equal 56
        dateTime.Millisecond |> should equal 0
