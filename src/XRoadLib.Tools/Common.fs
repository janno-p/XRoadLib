module XRoadLib.Tools.Common

open Microsoft.Extensions.DependencyInjection
open System.IO
open System.Reflection

type CommandLineOptions() =
    member val WsdlUri = "" with get, set
    member val IsVerbose = false with get, set
    member val IsCodeOutput = false with get, set
    member val OutputPath = Unchecked.defaultof<DirectoryInfo> with get, set
    member val MappingPath = Option<FileInfo>.None with get, set

type IAutoConfigure = interface end

let configureAutoServices (services: IServiceCollection) =
    typeof<IAutoConfigure>.GetTypeInfo().Assembly.GetTypes()
    |> Array.filter (fun typ -> typ.GetTypeInfo().IsAbstract |> not)
    |> Array.filter (typeof<IAutoConfigure>.IsAssignableFrom)
    |> Array.iter (fun typ ->
        match typ.GetInterfaces() |> Array.filter (fun iface -> iface.Name = sprintf "I%s" typ.Name) with
        | [| ifaceTyp |] -> services.AddSingleton(ifaceTyp, typ) |> ignore
        | _ -> ())

let notImplemented msg = failwithf "Not implemented: %s" msg
