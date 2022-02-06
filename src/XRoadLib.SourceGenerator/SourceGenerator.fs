namespace XRoadLib.SourceGenerator

open Microsoft.CodeAnalysis
open System
open System.IO
open System.Net.Http

module AdditionalFilesMetadata =
    [<Literal>]
    let private prefix = "build_metadata.AdditionalFiles."

    [<Literal>]
    let sourceGenerator = prefix + "XRoadSourceGenerator"
    [<Literal>]
    let wsdlUri = prefix + "WsdlUri"

type ServiceSource =
    {
        File: FileInfo
        WsdlUri: Uri option
    }

[<Generator(Microsoft.CodeAnalysis.LanguageNames.CSharp)>]
type XRoadLibSourceGenerator () =
    let hasWsdlExtension (additionalText: AdditionalText) =
        Path.GetExtension(additionalText.Path).Equals(".wsdl", StringComparison.OrdinalIgnoreCase)

    let getLoadOptions (context: GeneratorExecutionContext) =
        context.AdditionalFiles
        |> Seq.filter hasWsdlExtension
        |> Seq.choose (fun file ->
            let fileOptions = context.AnalyzerConfigOptions.GetOptions(file)

            match fileOptions.TryGetValue(AdditionalFilesMetadata.sourceGenerator) with
            | true, enabledString when enabledString.ToLower() = "true" ->
                let wsdlUri =
                    match fileOptions.TryGetValue(AdditionalFilesMetadata.wsdlUri) with
                    | true, wsdlUriString when not (String.IsNullOrWhiteSpace(wsdlUriString)) ->
                        Some(Uri(wsdlUriString))
                    | _ -> None
                Some({ File = FileInfo(file.Path); WsdlUri = wsdlUri })
            | _ -> None
        )

    let buildPort namespaceName (port: Wsdl.ServicePort) = [
        let ifaceSrc = Text.StringBuilder()
        ifaceSrc.AppendLine("using XRoadLib.Attributes;").AppendLine() |> ignore

        ifaceSrc.AppendLine($"namespace %s{namespaceName}").AppendLine("{") |> ignore
        ifaceSrc.AppendLine($"    public interface I%s{port.Name.LocalName}").AppendLine("    {") |> ignore

        port.Binding.Operations
        |> List.iteri (fun i op ->
            if i > 0 then ifaceSrc.AppendLine() |> ignore

            ifaceSrc.Append($"        [XRoadService(\"%s{op.PortOperation.Name}\"") |> ignore
            op.Version |> Option.iter (fun v -> ifaceSrc.Append($", \"%s{v}\"") |> ignore)
            ifaceSrc.AppendLine(")]") |> ignore

            ifaceSrc.AppendLine($"        void %s{op.PortOperation.Name}();") |> ignore
        )

        ifaceSrc.AppendLine("    }") |> ignore
        ifaceSrc.AppendLine("}") |> ignore

        yield ($"I%s{port.Name.LocalName}.cs", string ifaceSrc)
    ]

    let executeAsync context = task {
        for source in getLoadOptions context do
            match source.File.Exists, source.WsdlUri with
            | false, Some(wsdlUri) ->
                use file = source.File.Open(FileMode.CreateNew)
                use httpClient = new HttpClient()
                use! stream = httpClient.GetStreamAsync(wsdlUri)
                do! stream.CopyToAsync(file)
            | _ -> ()

            let doc = System.Xml.Linq.XDocument.Load(source.File.FullName)
            let wsdl = Wsdl.parse doc.Root (Uri source.File.FullName) null

            wsdl.Services
            |> Seq.iter
                (fun svc ->
                    svc.Ports
                    |> List.map (buildPort $"%s{Path.GetFileNameWithoutExtension(source.File.Name)}.%s{svc.Name}")
                    |> List.concat
                    |> List.iter context.AddSource
                )
    }

    let showCompilerError (context: GeneratorExecutionContext) (e: exn) =
        context.ReportDiagnostic(Diagnostic.Create("XRD0001", "Compiler", e.Message, DiagnosticSeverity.Error, DiagnosticSeverity.Error, true, 0))

    interface ISourceGenerator with
        member _.Execute(context) =
            try
                let task = executeAsync context
                task.Wait()
            with e ->
                match e with
                | :? AggregateException as e -> e.InnerExceptions |> Seq.iter (showCompilerError context)
                | e -> showCompilerError context e

        member _.Initialize(_) =
            ()
