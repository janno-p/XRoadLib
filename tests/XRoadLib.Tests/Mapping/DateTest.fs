namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System
open System.IO
open System.Xml
open XRoadLib.Protocols.Headers
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

        use message = new XRoadMessage(Globals.XRoadProtocol20, XRoadHeader20())

        typeMap.Deserialize(reader, null, Globals.TestDefinition(typeMap.Definition.Type), message)

[<TestFixture>]
module DateTest =
    open XRoadLib.Schema

    let schemaDefinitionReader = SchemaDefinitionReader("")
    let dateTypeMap = DateTypeMap(schemaDefinitionReader.GetSimpleTypeDefinition<DateTime>("date"))
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
        |> should (throwWithMessage @"String was not recognized as a valid DateTime.") typeof<FormatException>

    [<Test>]
    let ``cannot deserialize dateTime format`` () =
        TestDelegate(fun _ -> deserializeValue "2013-08-04T11:11:11" |> ignore)
        |> should (throwWithMessage @"String was not recognized as a valid DateTime.") typeof<FormatException>

    [<Test>]
    let ``deserializes universal timezone to local timezone`` () =
        let instance = deserializeValue "2013-08-27Z"
        instance |> should not' (be Null)
        let dateTime: DateTime = unbox instance
        dateTime.Kind |> should equal DateTimeKind.Local
        dateTime.Year |> should equal 2013
        dateTime.Month |> should equal 8
        dateTime.Day |> should equal 27
        dateTime.Hour |> should equal 3
        dateTime.Minute |> should equal 0
        dateTime.Second |> should equal 0
        dateTime.Millisecond |> should equal 0

    [<Test>]
    let ``deserializes explicit timezone to local timezone`` () =
        let instance = deserializeValue "2013-08-27-03:00"
        instance |> should not' (be Null)
        let dateTime: DateTime = unbox instance
        dateTime.Kind |> should equal DateTimeKind.Local
        dateTime.Year |> should equal 2013
        dateTime.Month |> should equal 8
        dateTime.Day |> should equal 27
        dateTime.Hour |> should equal 6
        dateTime.Minute |> should equal 0
        dateTime.Second |> should equal 0
        dateTime.Millisecond |> should equal 0
