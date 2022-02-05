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

    let executeAsync context = task {
        for source in getLoadOptions context do
            match source.File.Exists, source.WsdlUri with
            | false, Some(wsdlUri) ->
                use file = source.File.Open(FileMode.CreateNew)
                use httpClient = new HttpClient()
                use! stream = httpClient.GetStreamAsync(wsdlUri)
                do! stream.CopyToAsync(file)
            | _ -> ()
    }

    interface ISourceGenerator with
        member _.Execute(context) =
            let task = executeAsync context
            task.Wait()

        member _.Initialize(_) =
            ()
