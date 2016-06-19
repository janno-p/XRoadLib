namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System
open System.IO
open System.Xml
open System.Xml.Linq
open XRoadLib.Extensions
open XRoadLib.Serialization
open XRoadLib.Tests.Contract.Mapping

[<TestFixture>]
module ArrayTest =
    let protocol = Globals.XRoadProtocol31
    let serializer = protocol.GetSerializerCache(Nullable(1u))
    let xn nm = XName.Get(nm)

    let serializeMessage request =
        let serviceMap = serializer.GetServiceMap("MergeArrayContent")

        use message = new XRoadMessage(protocol, null)
        use stream =
            let stream = new MemoryStream()
            use writer = XmlWriter.Create(stream, XmlWriterSettings(CloseOutput = false))

            serviceMap.SerializeRequest(writer, request, message)
            writer.Flush()
            stream

        stream.Position <- 0L
        XDocument.Load(stream)

    let deserializeMessage (doc: XDocument) =
        let serviceMap = serializer.GetServiceMap("MergeArrayContent")

        let doc2 = XDocument(XElement(xn "envelope", XElement(xn "body", doc.Root)))

        use message = new XRoadMessage(protocol, null)
        use reader = doc2.CreateReader()

        (reader.MoveToElement(0) && reader.MoveToElement(1) && reader.MoveToElement(2)) |> ignore

        serviceMap.DeserializeRequest(reader, message)

    let request =
        let request = MergeArrayContentRequest()
        request.StartDate <- DateTime(2016, 2, 10)
        request.EndDate <- DateTime(2016, 2, 11)
        request.Content <- [| WrapperType(Integers = [| 10 |], Strings = [| "test" |])
                              WrapperType()
                              WrapperType(Integers = [| 1; 2; 3 |], Strings = [| "1"; "2"; "3" |]) |]
        request

    [<Test>]
    let ``can serialize merged array content`` () =
        let doc = serializeMessage request
        doc.Root.Name |> should equal (XName.Get("MergeArrayContent", Globals.XRoadProtocol31.ProducerNamespace))
        doc.Root.Elements() |> Seq.length |> should equal 1
        let req = doc.Root.Elements(XName.Get("request")) |> Seq.exactlyOne
        req.Elements() |> Seq.length |> should equal 5
        [ "StartDate"; "EndDate"; "Content"; "Content"; "Content" ]
        |> Seq.zip (req.Elements())
        |> Seq.iter (fun (el, nm) -> el.Name |> should equal (XName.Get(nm)))
        let sdt = req.Elements(XName.Get("StartDate")) |> Seq.exactlyOne
        sdt.Value |> should equal "2016-02-10"
        let edt = req.Elements(XName.Get("EndDate")) |> Seq.exactlyOne
        edt.Value |> should equal "2016-02-11"
        let cnt1, cnt2, cnt3 = match req.Elements(XName.Get("Content")) |> List.ofSeq with [a; b; c] -> a, b, c | _ -> failwith ""
        cnt1.Elements() |> Seq.length |> should equal 2
        [ ("Integer", "10"); ("String", "test")]
        |> Seq.zip (cnt1.Elements())
        |> Seq.iter (fun (el, (nm, v)) ->
            el.Name |> should equal (XName.Get(nm))
            el.Value |> should equal v)
        cnt2.Elements() |> Seq.length |> should equal 0
        cnt3.Elements() |> Seq.length |> should equal 6
        [ ("Integer", "1"); ("Integer", "2"); ("Integer", "3"); ("String", "1"); ("String", "2"); ("String", "3") ]
        |> Seq.zip (cnt3.Elements())
        |> Seq.iter (fun (el, (nm, v)) ->
            el.Name |> should equal (XName.Get(nm))
            el.Value |> should equal v)

    [<Test>]
    let ``can deserialize merged array content`` () =
        let result = request |> serializeMessage |> deserializeMessage
        result |> should not' (be Null)
        result |> should be instanceOfType<MergeArrayContentRequest>
        let req = unbox<MergeArrayContentRequest> result
        req.StartDate |> should equal (DateTime(2016, 2, 10))
        req.EndDate |> should equal (DateTime(2016, 2, 11))
        req.Content |> should not' (be Null)
        req.Content |> Array.length |> should equal 3
        let cnt1, cnt2, cnt3 = match req.Content with [| a; b; c |] -> a, b, c | _ -> failwith ""
        cnt1 |> should not' (be Null)
        cnt1.Integers |> should not' (be Null)
        cnt1.Integers |> should equal [| 10 |]
        cnt1.Strings |> should not' (be Null)
        cnt1.Strings |> should equal [| "test" |]
        cnt2 |> should not' (be Null)
        cnt2.Integers |> should be Null
        cnt2.Strings |> should be Null
        cnt3 |> should not' (be Null)
        cnt3.Integers |> should not' (be Null)
        cnt3.Integers |> should equal [| 1; 2; 3 |]
        cnt3.Strings |> should not' (be Null)
        cnt3.Strings |> should equal [| "1"; "2"; "3" |]
