module XRoadLib.Tools.CodeGen

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open Microsoft.CodeAnalysis.Formatting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open System
open System.Collections.Generic
open System.IO
open System.Net.Http
open System.Reflection
open System.Text
open System.Xml.Linq
open XRoadLib.Tools.Common
open XRoadLib.Tools.Parsing
open XRoadLib.Tools.Syntax

[<Interface>]
type IGenerator =
    abstract Run: unit -> Async<unit>

[<Interface>]
type IDefinitionBuilder =
    abstract Build: XDocument -> unit

[<Interface>]
type IServiceBuilder =
    abstract Build: XElement -> CompilationUnitSyntax

type Generator(optionsProvider: IOptions<CommandLineOptions>, loggerFactory: ILoggerFactory, definitionBuilder: IDefinitionBuilder) =
    let logger = loggerFactory.CreateLogger<ILogger<Generator>>()
    let options = optionsProvider.Value
    let loadSchemaAsync () = async {
        if Uri.IsWellFormedUriString(options.WsdlUri, UriKind.Absolute) then
            logger.LogInformation("Requesting service description from network location: {0}.", options.WsdlUri)
            use client = new HttpClient()
            use! stream = client.GetStreamAsync(options.WsdlUri) |> Async.AwaitTask
            return XDocument.Load(stream)
        else
            let fileInfo = FileInfo(options.WsdlUri)
            if fileInfo.Exists then
                logger.LogInformation("Requesting service description from file: {0}.", fileInfo.FullName)
                return XDocument.Load(fileInfo.FullName)
            else
                return failwithf "Cannot resolve WSDL location: %s" options.WsdlUri
    }
    interface IAutoConfigure
    interface IGenerator with
        member __.Run() = async {
            if (options.WsdlUri |> String.IsNullOrEmpty) then
                failwith "WSDL location url is required"
            let! document = loadSchemaAsync()
            definitionBuilder.Build(document)
        }

type DefinitionBuilder(optionsProvider: IOptions<CommandLineOptions>) =
    let options = optionsProvider.Value
    interface IAutoConfigure
    interface IDefinitionBuilder with
        member __.Build(document) =
            let definitions = document.Element(xnw "definitions") |> Object.except "Invalid WSDL document (definitions element is missing)."

            let mutable csAssemblyInfo =
                SourceFile.New(FileInfo(Path.Combine(options.OutputPath.FullName, "Properties", "AssemblyInfo.cs")))
                |> addUsing "System.Reflection"

            definitions.Element(xnw "documentation")
            |> Option.ofObj
            |> Option.iter (fun documentation -> csAssemblyInfo <- csAssemblyInfo |> addAssemblyDescription documentation.Value)

            if not (definitions.Elements(xnw "import") |> Seq.isEmpty) then notImplemented "wsdl:import"

            (*

            if not (definitions.Elements(xnw "types") |> Seq.isEmpty) then notImplemented "wsdl:types"
            if not (definitions.Elements(xnw "message") |> Seq.isEmpty) then notImplemented "wsdl:message"
            if not (definitions.Elements(xnw "portType") |> Seq.isEmpty) then notImplemented "wsdl:portType"
            if not (definitions.Elements(xnw "binding") |> Seq.isEmpty) then notImplemented "wsdl:binding"
            if not (definitions.Elements(xnw "service") |> Seq.isEmpty) then notImplemented "wsdl:service"
            *)

            csAssemblyInfo |> saveFile

type ServiceBuilder() =
    interface IAutoConfigure
    interface IServiceBuilder with
        member __.Build(element) =
            null
