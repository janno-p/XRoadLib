namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System
open System.IO
open System.Xml
open XRoadLib
open XRoadLib.Protocols
open XRoadLib.Serialization
open XRoadLib.Serialization.Mapping

[<AutoOpen>]
module MappingTestHelpers =
    let deserializeValue value (typeMap: ITypeMap) =
        use stream = new MemoryStream()
        use writer = new StreamWriter(stream)
        writer.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8""?>")
        writer.WriteLine(sprintf @"<value>%s</value>" value)
        writer.Flush()

        stream.Position <- 0L
        use reader = XmlReader.Create(stream)
        while reader.Read() && (reader.NodeType <> XmlNodeType.Element) do ()

        use message = new XRoadMessage(Globals.XRoadProtocol20)
        let context = SerializationContext(message, 1u)

        typeMap.Deserialize(reader, null, context)

[<TestFixture>]
module DateTest =
    let dateTypeMap = DateTypeMap(XRoadLib.Schema.TypeDefinition.SimpleTypeDefinition<DateTime>("date"))
    let deserializeValue x = dateTypeMap |> deserializeValue x

    [<Test>]
    let ``can deserialize plain date`` () =
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
    let ``cannot deserialize wrong format`` () =
        TestDelegate(fun _ -> deserializeValue "2013-08-40" |> ignore)
        |> should (throwWithMessage @"The string '2013-08-40' is not a valid AllXsd value.") typeof<FormatException>

    [<Test>]
    let ``deserialization ignores time zone Z value`` () =
        let instance = deserializeValue "2013-08-27Z"
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
    let ``deserialization ignores any time zone value`` () =
        let instance = deserializeValue "2013-08-27-03:00"
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
