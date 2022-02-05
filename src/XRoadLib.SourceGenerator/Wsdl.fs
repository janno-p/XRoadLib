module XRoadLib.SourceGenerator.Wsdl

open System
open System.Xml
open System.Xml.Linq

module Namespace =
    let Soap = XNamespace.Get("http://schemas.xmlsoap.org/wsdl/soap/")
    let Wsdl = XNamespace.Get("http://schemas.xmlsoap.org/wsdl/")

let resolveXName (e: XElement) (name: string) =
    match name.Split(':') with
    | [| pfx; localName |] ->
        let ns = e.GetNamespaceOfPrefix(pfx)
        Some (ns + localName)
    | _ -> None

let (|XName|_|) e name = resolveXName e name

type BindingStyle = Document | Rpc

type MessageTarget =
    | SchemaType of XName
    | SchemaElement of XName

type Part =
    {
        Name: string
        Target: MessageTarget
    }

type Message =
    {
        Name: XName
        Part: Part option
    }

type PortOperation =
    {
        Name: string
        Documentation: XElement option // TODO!
        Input: Part option
        Output: Part option
    }

type PortType =
    {
        Name: XName
        Operations: PortOperation list
    }

type Operation =
    {
        SoapAction: string
        PortOperation: PortOperation
    }

type ServiceBinding =
    {
        Name: XName
        Operations: Operation list
        Style: BindingStyle
    }

type ServicePort =
    {
        Name: XName
        Location: string
        Binding: ServiceBinding
    }

type Service =
    {
        Name: string
        Ports: ServicePort list
    }

type Wsdl =
    {
        Types: obj list
        Services: Service list
    }

let parseMessage (tns: XNamespace) (message: XElement) =
    let name = message.Attribute(XName.Get "name").Value
    { Message.Name = tns + name; Part = None }

let parseMessages tns (definitions: XElement) =
    [ for e in definitions.Elements(Namespace.Wsdl + "message") ->
        parseMessage tns e
    ]

let parseOperation findMessage (operation: XElement) =
    let name = operation.Attribute(XName.Get "name").Value
    let documentation =
        operation.Element(Namespace.Wsdl + "documentation")
        |> Option.ofObj
    let input =
        let el = operation.Element(Namespace.Wsdl + "input")
        match el.Attribute(XName.Get "message").Value with
        | XName el name -> findMessage name
        | n -> failwith $"Could not find input message '%s{n}'"
    let output =
        let el = operation.Element(Namespace.Wsdl + "output")
        match el.Attribute(XName.Get "message").Value with
        | XName el name -> findMessage name
        | n -> failwith $"Could not find output message '%s{n}'"
    { Name = name; Documentation = documentation; Input = input.Part; Output = output.Part }

let parsePortTypes findMessage (tns: XNamespace) (definitions: XElement) =
    [ for e in definitions.Elements(Namespace.Wsdl + "portType") ->
        let name = e.Attribute(XName.Get "name").Value
        {
            PortType.Name = tns + name
            Operations =
                [ for op in e.Elements(Namespace.Wsdl + "operation") ->
                    parseOperation findMessage op
                ]
        }
    ]

let parsePort findBinding (tns: XNamespace) (serviceEl: XElement) =
    serviceEl.Element(Namespace.Soap + "address")
    |> Option.ofObj
    |> Option.map (fun address ->
        let name = serviceEl.Attribute(XName.Get "name").Value
        let location = address.Attribute(XName.Get "location").Value
        let bindingName =
            match serviceEl.Attribute(XName.Get "binding").Value with
            | XName serviceEl n -> n
            | t -> XName.Get t
        let binding = findBinding bindingName
        { Name = tns + name; Location = location; Binding = binding }
    )

let parseBinding (findPortType: XName -> PortType) (tns: XNamespace) (binding: XElement) =
    let name = binding.Attribute(XName.Get "name").Value
    let typ =
        match binding.Attribute(XName.Get "type").Value with
        | XName binding t -> t
        | t -> XName.Get t
    let portType = findPortType typ
    let operations =
        [ for operation in binding.Elements(Namespace.Wsdl + "operation") do
            let operationName = operation.Attribute(XName.Get "name").Value
            match operation.Element(Namespace.Soap + "operation") with
            | null -> ()
            | soapOperation ->
                let soapAction = soapOperation.Attribute(XName.Get "soapAction").Value
                let portOperation = portType.Operations |> List.find (fun x -> x.Name = operationName)
                { SoapAction = soapAction; PortOperation = portOperation }
        ]
    let style =
        match binding.Attribute(XName.Get "style") |> Option.ofObj |> Option.map (fun x -> x.Value) with
        | Some "rpc" -> Rpc
        | _ -> Document
    { Name = tns + name; Operations = operations; Style = style }

let parseBindings findPortType tns (definitions: XElement) =
    [ for e in definitions.Elements(Namespace.Wsdl + "binding") ->
        parseBinding findPortType tns e
    ]

let parseServices findBinding tns (definitions: XElement) =
    [ for e in definitions.Elements(Namespace.Wsdl + "service") ->
        {
            Name = e.Attribute(XName.Get "name").Value
            Ports =
                e.Elements(Namespace.Wsdl + "port")
                |> Seq.choose (parsePort findBinding tns)
                |> Seq.toList
        }
    ]

let parse (definitions: XElement) (documentUri: Uri) (resolver: XmlResolver) =
    let tns = definitions.Attribute(XName.Get "targetNamespace").Value |> XNamespace.Get

    let messages = definitions |> parseMessages tns
    let findMessage name = messages |> List.find (fun x -> x.Name = name)

    let portTypes = definitions |> parsePortTypes findMessage tns
    let findPortType name = portTypes |> List.find (fun x -> x.Name = name)

    let bindings = definitions |> parseBindings findPortType tns
    let findBindings name = bindings |> List.find (fun x -> x.Name = name)

    let services = definitions |> parseServices findBindings tns

    { Wsdl.Services = services; Types = [] }
