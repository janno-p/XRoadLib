Sample tutorial
========================

In this tutorial we will create simple X-Road service and set up a complete infrastructure to make that service
available for X-Road infrastucture. This includes:

* Describe types and methods which describe and implement X-Road operation.

* Set up service request handler which listens for incoming messages and executes specified operations.

* Set up service description handler to provide WSDL document which represents technical specification for the
  X-Road operation.

Create new solution
------------------------

Create new empty ASP.NET Web application `Calculator` in Visual Studio. XRoadLib is designed to be used with ASP.NET
applications, but it shouldn't be difficult to use it in alternative setups. This tutorial assumes we're working on
ASP.NET Web application. Add reference to `XRoadLib` in web project.

Contract library
------------------------

Create empty library project `Calculator.Contract` to the solution, which defines service contracts for our
application. Add reference to XRoadLib library using NuGet or local assembly file. In this project create new class
`AddRequest` which represents main request type for our X-Road service.

    [lang=csharp]
    using System.Xml.Serialization;
    using XRoadLib.Serialization;
    
    namespace Calculator.Contract
    {
        public class AddRequest : XRoadSerializable
        {
            [XmlElement(Order = 1)]
            public int X { get; set; }
            
            [XmlElement(Order = 2)]
            public int Y { get; set; }
        }
    }

Defined class describes `request` part of our operation message, which has two integer type arguments `X` and `Y`.
XRoadLib defines its types as XML Schema sequences, which means the order of elements is important. By default XRoadLib
orders elements by name. To use specific order we use `System.Xml.Serialization.XmlElementAttribute` attribute `Order`
property. More advanced alternative to sort type properties, is to provide custom comparer for `TypeDefinition` through
`ISchemaExporter` interface.

Next we need to define contract for operation itself. For that create new interface in contract project named
`ISumOfIntegers`.

    [lang=csharp]
    using XRoadLib.Attributes;
    
    namespace Calculator.Contract
    {
        public interface ISumOfIntegers
        {
            [XRoadService("SumOfIntegers")]
            int Sum(AddRequest request);
        }
    }

In here we defined new operation contract with input of type `AddRequest` and output of type `integer`. Currently XRoadLib
allows at most 1 parameter for method interface which represents operation contract. Attribute
`XRoadLib.Attributes.XRoadServiceAttribute` defines service code for X-Road operation which will be used in X-Road header
to identify the service.

Custom X-Road protocol
------------------------

XRoadLib supports different X-Road message protocol versions, and allows to extend them to add provider specific details
and configurations. Add new class to web project `CalculatorXRoadProtocol`:

    [lang=csharp]
    using Calculator.Contract;
    using XRoadLib.Protocols;

    namespace Calculator.WebService
    {
        public class CalculatorXRoadProtocol : XRoad31Protocol
        {
            public static readonly XRoadProtocol Instance = new CalculatorXRoadProtocol();

            private CalculatorXRoadProtocol()
                : base("calculator", "http://calculator.x-road.eu/producer/")
            { }

            static CalculatorXRoadProtocol()
            {
                Instance.SetContractAssembly(typeof(AddRequest).Assembly, null);
            }
        }
    }

Here we specify that our Calculator application is based on X-Road message protocol version 3.1. We define our producer
name and namespace. Since XRoadProtocol caches type specific information, its necessary to follow singleton pattern
when creating instance of the protocol. Also, we need to bind contract assembly to protocol.

If required, same web application can support multiple X-Road message protocols and have separate configurations for
individual protocol instances.

Service contract handler
------------------------

In this section we're going to create new handler which generates WSDL document according to service contract. Add new
generic handler `wsdl.ashx` to the web project:

    [lang=csharp]
    using XRoadLib.Handler;
    using XRoadLib.Protocols;

    namespace Calculator.WebService
    {
        public class Wsdl : ServiceDescriptionHandlerBase
        {
            protected override XRoadProtocol Protocol => CalculatorXRoadProtocol.Instance;
        }
    }

`Wsdl` handler derives from `XRoadLib.Handler.ServiceDescriptionHandlerBase` which implements minimal service
description handler for X-Road web service. To generate correct WSDL documentation, service description handler
requires reference to implementing X-Road message protocol.

After this step we can compile the project and navigate to service description handler with web browser, which should
display valid service description document for our contract.

Service request handler
------------------------

Final step in our small tutorial is to create handler which serves incoming requests. Since we're missing the
implementation of the service, lets add new class to web project named `SumOfIntegersWebService`:

    [lang=csharp]
    using Calculator.Contract;

    namespace Calculator.WebService
    {
        public class SumOfIntegersWebService : ISumOfIntegers
        {
            public int Sum(AddRequest request)
            {
                return request.X + request.Y;
            }
        }
    }

Last step is to create service request handler, which brings together service contract definition and its
implementation. Add new generic handler `Calculator.ashx` to your web project:

    [lang=csharp]
    using System;
    using Calculator.WebService;
    using XRoadLib.Handler;
    using XRoadLib.Serialization;

    namespace Calculator.WebService
    {
        public class Calculator : ServiceRequestHandlerBase
        {
            public Calculator()
                : base(new[] { CalculatorXRoadProtocol.Instance })
            { }

            protected override object InvokeMetaService(MetaServiceName metaServiceName)
            {
                throw new NotImplementedException();
            }

            protected override object GetServiceObject(string operationName)
            {
                if (operationName == "SumOfIntegers")
                    return new SumOfIntegersWebService();

                throw new NotImplementedException();
            }
        }
    }

Base handlers expects list of all supported protocols as constructor arguments which will be used to detect
message protocol version of incoming service request.

Our simple example won't support any meta services, so we leave that with default implementation which returns
exception. We're only supporting one functional service at the moment, so for the simplicity we're initializing
it every time by creating new instance of the service object.

After that, our simple web application is ready to accept incoming X-Road requests.

Sample request
------------------------

    [lang=xml]
    <?xml version="1.0" encoding="utf-8"?>
    <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                      xmlns:xrd="http://x-road.ee/xsd/x-road.xsd"
                      xmlns:calc="http://calculator.x-road.eu/producer/">
      <soapenv:Header>
        <xrd:consumer>1234</xrd:consumer>
        <xrd:producer>calculator</xrd:producer>
        <xrd:service>calculator.SumOfIntegers.v1</xrd:service>
        <xrd:id>ABCDE</xrd:id>
        <xrd:userId>30101010001</xrd:userId>
        <xrd:userName>toomas.dumpti</xrd:userName>
      </soapenv:Header>
      <soapenv:Body>
        <calc:SumOfIntegers>
          <request>
            <X>12</X>
            <Y>7</Y>
          </request>
        </calc:SumOfIntegers>
      </soapenv:Body>
    </soapenv:Envelope>

Sample response
------------------------

    [lang=xml]
    <?xml version="1.0"?>
    <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                      xmlns:xrd="http://x-road.ee/xsd/x-road.xsd"
                      xmlns:calc="http://calculator.x-road.eu/producer/">
      <soapenv:Header>
        <xrd:consumer>1234</xrd:consumer>
        <xrd:producer>calculator</xrd:producer>
        <xrd:service>calculator.SumOfIntegers.v1</xrd:service>
        <xrd:id>ABCDE</xrd:id>
        <xrd:userId>30101010001</xrd:userId>
        <xrd:userName>toomas.dumpti</xrd:userName>
      </soapenv:Header>
      <soapenv:Body>
        <calc:SumOfIntegersResponse>
          <request>
            <X>12</X>
            <Y>7</Y>
          </request>
          <response>
            <result>19</result>
          </response>
        </calc:SumOfIntegersResponse>
      </soapenv:Body>
    </soapenv:Envelope>
