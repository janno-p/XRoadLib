module XRoadLib.Tools.Program

open Microsoft.Extensions.CommandLineUtils;
open Microsoft.Extensions.DependencyInjection;
open Microsoft.Extensions.Logging
open System
open System.IO
open System.Reflection
open XRoadLib.Tools.CodeGen
open XRoadLib.Tools.Common
open XRoadLib.Tools.Parsing

let loggerFactory = (new LoggerFactory()).AddConsole()
let logger = loggerFactory.CreateLogger<ILogger>()

let configureServices (fillCommandLineOptions: CommandLineOptions -> unit) =
    let services = ServiceCollection()
    services |> configureAutoServices
    services.AddOptions()
            .Configure<CommandLineOptions>(fillCommandLineOptions)
            .AddSingleton<ILoggerFactory>(loggerFactory)
            .BuildServiceProvider()

let getInformationalVersion () =
    let assembly = typeof<IGenerator>.GetTypeInfo().Assembly
    let attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
    attribute |> Option.ofObj |> Option.fold (fun _ attr -> attr.InformationalVersion) (assembly.GetName().Version.ToString())

[<EntryPoint>]
let main args =
    try
        let app = CommandLineApplication()
        app.Name <- "dotnet-xroad-gen"
        app.Description <- "XRoadLib code generator"
        app.ShortVersionGetter <- (fun _ -> getInformationalVersion())

        app.HelpOption("-?|-h|--help") |> ignore

        let optVerbose = app.Option("-v|--verbose", "Verbose output", CommandOptionType.NoValue)
        let optSource = app.Argument("[wsdl]", "Url of service description file")
        let optCode = app.Option("-c|--code", "Generate code", CommandOptionType.NoValue)
        let optOutput = app.Option("-o|--output", "Output path", CommandOptionType.SingleValue)
        let optMapping = app.Option("-m|--mapping", "Customization mappings for generated code", CommandOptionType.SingleValue)

        let fillCommandLineOptions (opt: CommandLineOptions) =
            opt.WsdlUri <- optSource.Value
            opt.IsVerbose <- optVerbose.HasValue()
            opt.IsCodeOutput <- optCode.HasValue()
            opt.MappingPath <- if optMapping.HasValue() then Some(FileInfo(optMapping.Value())) else None
            opt.OutputPath <- DirectoryInfo(if optOutput.HasValue() then optOutput.Value() else "Output")

        let serviceProvider = fillCommandLineOptions |> configureServices

        let run = async {
            let generator = serviceProvider.GetRequiredService<IGenerator>()
            do! generator.Run()
            return 0
        }

        app.OnExecute(fun () -> run |> Async.StartAsTask)

        app.Execute(args)
    with ex ->
        logger.LogCritical(EventId(), ex, ex.Message)
        1
