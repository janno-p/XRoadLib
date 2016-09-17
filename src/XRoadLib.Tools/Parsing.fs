module XRoadLib.Tools.Parsing

open Microsoft.Extensions.Logging
open System
open System.IO
open System.Net.Http
open System.Xml.Linq
open XRoadLib
open XRoadLib.Tools.Common

type XElement with
    member this.HasAttribute(attributeName) =
        this.Attribute(XName.Get(attributeName)) |> isNotNull

[<AutoOpen>]
module XElement =
    let hasAttribute attributeName (element: XElement) = element.HasAttribute(attributeName)

type XElementParser(element: XElement, ?ns) as this =
    let ns = defaultArg ns NamespaceConstants.XSD
    let mutable isDone = true
    let enumerator = element.Elements().GetEnumerator()
    let moveToNext () = match enumerator.MoveNext() with true -> isDone <- false; true | x -> x
    member __.IsDone with get() = isDone
    member __.AttributeNotImplemented(attributeName) =
        if element |> hasAttribute attributeName then attributeName |> notImplemented
    member __.ParseElement(name, ?action: XElement -> unit) =
        if isDone && (moveToNext() |> not) then false
        elif enumerator.Current |> isNotNull && enumerator.Current.Name = XName.Get(name, ns) then
            if action |> Option.isNone then enumerator.Current.Name.ToString() |> notImplemented
            action |> Option.iter (fun f -> f(enumerator.Current))
            isDone <- true
            true
        else false
    member __.ThrowIfNotDone() =
        if not isDone then enumerator.Current.Name.ToString() |> notImplemented
    interface IDisposable with
        member __.Dispose() = this.ThrowIfNotDone()

let xn nm = XName.Get(nm)
let xns nm ns = XName.Get(nm, ns)
let xnw nm = xns nm NamespaceConstants.WSDL

module Option =
    let except msg o = match o with Some(v) -> v | None -> failwith msg

module Object =
    let except msg o = o |> Option.ofObj |> Option.except msg
