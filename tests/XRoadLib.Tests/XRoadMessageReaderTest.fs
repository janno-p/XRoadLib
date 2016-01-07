namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System.Collections.Specialized
open System.IO
open System.Text
open XRoadLib.Serialization

[<TestFixture>]
module XRoadMessageReaderTest =
    [<Test>]
    let ``can handle buffer limit`` () =
        use stream = new MemoryStream(Array.init 10 (fun _ -> 32uy))
        use reader = new XRoadMessageReader(stream, new NameValueCollection(), Encoding.UTF8, Path.GetTempPath())
        stream.Position <- 0L

        let mutable chunk = [| |]

        reader.ReadChunkOrLine(&chunk, 3) |> should equal XRoadMessageReader.ChunkStop.BufferLimit
        chunk |> should equal [| 32uy; 32uy; 32uy |]

        reader.ReadChunkOrLine(&chunk, 3) |> should equal XRoadMessageReader.ChunkStop.BufferLimit
        chunk |> should equal [| 32uy; 32uy; 32uy |]

        reader.ReadChunkOrLine(&chunk, 3) |> should equal XRoadMessageReader.ChunkStop.BufferLimit
        chunk |> should equal [| 32uy; 32uy; 32uy |]

        reader.ReadChunkOrLine(&chunk, 3) |> should equal XRoadMessageReader.ChunkStop.EndOfStream
        chunk |> should equal [| 32uy |]

    [<Test>]
    let ``can handle line marker`` () =
        use stream = new MemoryStream([| 32uy; 32uy; 32uy; 32uy; 13uy; 10uy; 32uy; 32uy; 32uy; 32uy |])
        use reader = new XRoadMessageReader(stream, new NameValueCollection(), Encoding.UTF8, Path.GetTempPath())
        stream.Position <- 0L

        let mutable chunk = [| |]

        reader.ReadChunkOrLine(&chunk, 10) |> should equal XRoadMessageReader.ChunkStop.NewLine
        chunk |> should equal [| 32uy; 32uy; 32uy; 32uy |]

        reader.ReadChunkOrLine(&chunk, 10) |> should equal XRoadMessageReader.ChunkStop.EndOfStream
        chunk |> should equal [| 32uy; 32uy; 32uy; 32uy |]

    [<Test>]
    let ``can handle chunk beginning with marker`` () =
        use stream = new MemoryStream([| 32uy; 32uy; 32uy; 32uy; 13uy; 10uy; 32uy; 32uy; 32uy; 32uy |])
        use reader = new XRoadMessageReader(stream, new NameValueCollection(), Encoding.UTF8, Path.GetTempPath())
        stream.Position <- 0L

        let mutable chunk = [| |]

        reader.ReadChunkOrLine(&chunk, 4) |> should equal XRoadMessageReader.ChunkStop.BufferLimit
        chunk |> should equal [| 32uy; 32uy; 32uy; 32uy |]

        reader.ReadChunkOrLine(&chunk, 4) |> should equal XRoadMessageReader.ChunkStop.NewLine
        chunk |> should equal [| |]

        reader.ReadChunkOrLine(&chunk, 4) |> should equal XRoadMessageReader.ChunkStop.BufferLimit
        chunk |> should equal [| 32uy; 32uy; 32uy; 32uy |]

        reader.ReadChunkOrLine(&chunk, 4) |> should equal XRoadMessageReader.ChunkStop.EndOfStream
        chunk |> should equal [| |]

    [<Test>]
    let ``can handle splitting marker`` () =
        use stream = new MemoryStream([| 32uy; 32uy; 32uy; 32uy; 13uy; 10uy; 32uy; 32uy; 32uy; 32uy |])
        use reader = new XRoadMessageReader(stream, new NameValueCollection(), Encoding.UTF8, Path.GetTempPath())
        stream.Position <- 0L

        let mutable chunk = [| |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.NewLine
        chunk |> should equal [| 32uy; 32uy; 32uy; 32uy |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.EndOfStream
        chunk |> should equal [| 32uy; 32uy; 32uy; 32uy |]

    [<Test>]
    let ``can handle multiple markers in a row`` () =
        use stream = new MemoryStream([| 40uy; 13uy; 10uy; 13uy; 10uy; 13uy; 10uy; 13uy; 10uy; 40uy |])
        use reader = new XRoadMessageReader(stream, new NameValueCollection(), Encoding.UTF8, Path.GetTempPath())
        stream.Position <- 0L

        let mutable chunk = [| |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.NewLine
        chunk |> should equal [| 40uy |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.NewLine
        chunk |> should equal [| |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.NewLine
        chunk |> should equal [| |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.NewLine
        chunk |> should equal [| |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.EndOfStream
        chunk |> should equal [| 40uy |]

    [<Test>]
    let ``can handle recurring marker buffer limit`` () =
        use stream = new MemoryStream([| 40uy; 13uy; 13uy; 13uy; 13uy; 13uy; 13uy; 10uy; 33uy; 34uy; 40uy; 40uy |])
        use reader = new XRoadMessageReader(stream, new NameValueCollection(), Encoding.UTF8, Path.GetTempPath())
        stream.Position <- 0L

        let mutable chunk = [| |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.BufferLimit
        chunk |> should equal [| 40uy; 13uy; 13uy; 13uy; 13uy |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.NewLine
        chunk |> should equal [| 13uy |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.EndOfStream
        chunk |> should equal [| 33uy; 34uy; 40uy; 40uy |]
