namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open XRoadLib
open XRoadLib.Serialization
open System.Collections.Specialized
open System.IO
open System.Text

[<TestFixture>]
module ContentLengthTest =
    let writeContent (content, attachments) =
        use outputStream = new MemoryStream()
        use contentStream = new MemoryStream()
        content |> Option.iter (fun value ->
            let buf = Encoding.UTF8.GetBytes(value: string)
            contentStream.Write(buf, 0, buf.Length))
        use message = new XRoadMessage(contentStream)
        attachments |> List.iter message.AllAttachments.Add
        message.SaveTo(outputStream, (fun _ -> ()), (fun _ _ -> ()))
        message.ContentLength

    [<Test>]
    let ``write empty content`` () =
        (None, []) |> writeContent |> should equal 0

    [<Test>]
    let ``write content without attachments`` () =
        (Some("test"), []) |> writeContent |> should equal 4

    [<Test>]
    let ``write content with attachments`` () =
        use x = new XRoadAttachment(new MemoryStream([| 1uy; 2uy; 3uy; 4uy |]))
        use y = new XRoadAttachment(new MemoryStream([| 5uy; 6uy; 7uy; 8uy; 9uy |]))
        (Some("test2"), [x; y]) |> writeContent |> should equal 534

    [<Test>]
    let ``read empty content without attachments`` () =
        use stream = new MemoryStream()
        use reader = new XRoadMessageReader(stream, NameValueCollection(), Encoding.UTF8, Path.GetTempPath(), [Globals.XRoadProtocol20; Globals.XRoadProtocol31; Globals.XRoadProtocol40])
        use msg = new XRoadMessage()
        reader.Read(msg, false)
        msg.ContentLength |> should equal 0L

    [<Test>]
    let ``read content without attachments`` () =
        use stream = new MemoryStream()
        use streamWriter = new StreamWriter(stream, Encoding.UTF8)
        streamWriter.Write("""<Envelope xmlns="http://schemas.xmlsoap.org/soap/envelope/" xmlns:id="http://x-road.eu/xsd/identifiers" xmlns:repr="http://x-road.eu/xsd/representation.xsd">
  <Header xmlns:xrd="http://x-road.ee/xsd/x-road.xsd">
  </Header>
  <Body />
</Envelope>""")
        streamWriter.Flush()
        stream.Position <- 0L
        use reader = new XRoadMessageReader(stream, NameValueCollection(), Encoding.UTF8, Path.GetTempPath(), [Globals.XRoadProtocol20; Globals.XRoadProtocol31; Globals.XRoadProtocol40])
        use msg = new XRoadMessage()
        reader.Read(msg, false)
        msg.ContentLength |> should equal 254L

    [<Test>]
    let ``read content with attachments`` () =
        use stream = new MemoryStream()
        use streamWriter = new StreamWriter(stream, Encoding.UTF8)
        streamWriter.Write("""
--5e7a8827-5850-45be-9a1e-8bbf6aff5da7
Content-Type: text/xml; charset=UTF-8
Content-Transfer-Encoding: 8bit
Content-ID: <rQI0gpIFuQMxlrqBj3qHKw==>

<?xml version="1.0" encoding="utf-8"?>
<Envelope xmlns="http://schemas.xmlsoap.org/soap/envelope/" xmlns:id="http://x-road.eu/xsd/identifiers" xmlns:repr="http://x-road.eu/xsd/representation.xsd">
  <Header xmlns:xrd="http://x-road.ee/xsd/x-road.xsd">
  </Header>
  <Body />
</Envelope>

--5e7a8827-5850-45be-9a1e-8bbf6aff5da7
Content-Type: application/octet-stream
Content-Transfer-Encoding: binary
Content-ID: <CNbAWiFRKnmh3+udKo8mLw==>

proovikas
--5e7a8827-5850-45be-9a1e-8bbf6aff5da7
Content-Type: application/octet-stream
Content-Transfer-Encoding: binary
Content-ID: <qrOlKraewrdRAu86cFnqwg==>

testikas sisu
--5e7a8827-5850-45be-9a1e-8bbf6aff5da7--""")
        streamWriter.Flush()
        stream.Position <- 0L
        let headers = NameValueCollection()
        headers.Add("Content-Type", "multipart/related; type=\"application/xml+xop\"; start=\"rQI0gpIFuQMxlrqBj3qHKw==\"; boundary=\"5e7a8827-5850-45be-9a1e-8bbf6aff5da7\"")
        use reader = new XRoadMessageReader(stream, headers, Encoding.UTF8, Path.GetTempPath(), [Globals.XRoadProtocol20; Globals.XRoadProtocol31; Globals.XRoadProtocol40])
        use msg = new XRoadMessage()
        reader.Read(msg, false)
        msg.ContentLength |> should equal 838L
