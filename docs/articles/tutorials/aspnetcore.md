Getting Started on .NET Core
============================

In this tutorial we will create simple X-Road service and small AspNetCore application to make that service
available for X-Road infrastucture. This includes:

* Describe types and methods which describe and implement X-Road operation.

* Configure `XRoadLibMiddleware` to handle service requests to provide service description in WSDL format.


Create AspNetCore web project
-----------------------------

This tutorial uses dotnet command line interface to create and manage dotnet projects, but these commands
are easily repeatable in Visual Studio using user interface.

Create new empty dotnet web application `Calculator`, and add reference to `XRoadLib` packages.

    $ mkdir Calculator
    $ cd Calculator
    $ dotnet new web
    $ dotnet add package XRoadLib
    $ dotnet add package XRoadLib.Extensions.AspNetCore


Service and type contracts
--------------------------

Add new `Contract` subfolder to the project and add definitions for the X-Road services and all the types
the service needs to operate.

First, create new class `AddRequest` which represents main request type for our X-Road service.

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


Service implementation
----------------------

Create new class which represents web service, that implements our `SumOfIntegers` X-Road operation:

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


Service request handler
------------------------

Next step is to define service manager class, which combines together service contract definition, its
implementation and X-Road protocol info. In our sample application we can rely on dependency injection
to provide implementations for the requested services.

    [lang=csharp]
    using XRoadLib;
    using XRoadLib.Headers;
    using XRoadLib.Schema;

    namespace Calculator
    {
        public class CalculatorServiceManager : ServiceManager<XRoadHeader>
        {
            public CalculatorServiceManager()
                : base("4.0", new DefaultSchemaExporter("http://calculator.x-road.eu/", typeof(CalculatorServiceManager).Assembly))
            { }
        }
    }

Arguments provided to base type constructor are unique name for defined service manager class and implementation
of IServiceManager interfaces which provides serialization and deserialization details of the services.
Current sample uses DefaultSchemaExporter which uses X-Road message protocol version 4.0 with default
settings.

After that, we have completed business logic of our simple web application and we can continue with technical setup.


Choosing X-Road protocol
------------------------

Interpretation of the service contract in X-Road context is determined by X-Road protocol version. Out of box XRoadLib
provides default implementations for X-Road protocol versions 2.0, 3.1 and 4.0. To use certain X-Road protocol version
you have to initialize `XRoadLib.ServiceManager` type instance with corresponding `SchemaExporter` implementation.
XRoadLib provides following schema exporters which correspond to certain X-Road protocol version:

* `XRoadLib.Schema.SchemaExporterXRoad20` -> X-Road protocol version 2.0 (legacy RPC/Encoded)
* `XRoadLib.Schema.SchemaExporterXRoad31` -> X-Road protocol version 3.1 (legacy Document/Literal)
* `XRoadLib.Schema.DefaultSchemaExporter` -> X-Road protocol version 4.0 (latest version)

Predefined protocol implementations can be further extended by overriding schema exporter methods and customizing its
behavior to suit your needs.


Startup class
-------------

In AspNetCore Startup class we have to register our service with dependency injection and configure
XRoadLibMiddleware to be used with our AspNetCore application.

    [lang=csharp]
    using Calculator.Contract;
    using Calculator.WebService;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using XRoadLib.Extensions.AspNetCore;

    namespace Calculator
    {
        public class Startup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddXRoadLib();

                services.AddSingleton<ICalculate, CalculateWebService>();
                services.AddSingleton<ISumOfIntegers, SumOfIntegersWebService>();
                services.AddSingleton<CalculatorServiceManager>();
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {
                if (env.IsDevelopment())
                    app.UseDeveloperExceptionPage();

                app.UseXRoadLib(routes =>
                {
                    routes.MapWsdl<CalculatorServiceManager>("");
                    routes.MapWebService<CalculatorServiceManager>("");
                });

                app.Run(async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            }
        }
    }


To use XRoadLib with AspNetCore platform the `IApplicationBuilder` extension method `UseXRoadLib` should
be called in `Startup` class of the application. Extension method accepts callback that allows to define
routes for service description (WSDL) and web service endpoints which provide access to functionality
of the services defined in specified service manager.

In the `ConfigureServices` method of the `Startup` class, XRoadLib has to be registered with the dependency
injection provider, by calling the `IServiceCollection` extension method `AddXRoadLib`. Since service
manager caches type specific information, its necessary to follow singleton pattern when registering
instance of the service manager type.

If required, same web application can support multiple X-Road message protocols and have separate configurations
for individual protocol instances. In such case each protocol should define its own separate service
manager type which should be registered through unique routes.

With this setup our application expects incoming requests to application root using HTTP "POST" method.
HTTP "GET" method to our application root returns service description in WSDL format. All other routes
pass through XRoadLib middleware and return "Hello World!" response.


Sample request
------------------------

    [lang=xml]
    <?xml version="1.0" encoding="utf-8"?>
    <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                      xmlns:xrd="http://x-road.eu/xsd/xroad.xsd"
                      xmlns:id="http://x-road.eu/xsd/identifiers"
                      xmlns:calc="http://calculator.x-road.eu/">
      <soapenv:Header>
        <xrd:client id:objectType="SUBSYSTEM">
          <id:xRoadInstance>EE</id:xRoadInstance>
          <id:memberClass>GOV</id:memberClass>
          <id:memberCode>00000000</id:memberCode>
          <id:subsystemCode>client</id:subsystemCode>
        </xrd:client>
        <xrd:service id:objectType="SERVICE">
          <id:xRoadInstance>EE</id:xRoadInstance>
          <id:memberClass>GOV</id:memberClass>
          <id:memberCode>11111111</id:memberCode>
          <id:subsystemCode>calculator</id:subsystemCode>
          <id:serviceCode>SumOfIntegers</id:serviceCode>
          <id:serviceVersion>v1</id:serviceVersion>
        </xrd:service>
        <xrd:id>ABCDE</xrd:id>
        <xrd:userId>30101010001</xrd:userId>
        <xrd:issue>12345</xrd:issue>
        <xrd:protocolVersion>4.0</xrd:protocolVersion>
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
    <?xml version="1.0" encoding="utf-8"?>
    <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                      xmlns:xrd="http://x-road.eu/xsd/xroad.xsd"
                      xmlns:id="http://x-road.eu/xsd/identifiers"
                      xmlns:calc="http://calculator.x-road.eu/">
      <soapenv:Header>
        <xrd:client id:objectType="SUBSYSTEM">
          <id:xRoadInstance>EE</id:xRoadInstance>
          <id:memberClass>GOV</id:memberClass>
          <id:memberCode>00000000</id:memberCode>
          <id:subsystemCode>client</id:subsystemCode>
        </xrd:client>
        <xrd:service id:objectType="SERVICE">
          <id:xRoadInstance>EE</id:xRoadInstance>
          <id:memberClass>GOV</id:memberClass>
          <id:memberCode>11111111</id:memberCode>
          <id:subsystemCode>calculator</id:subsystemCode>
          <id:serviceCode>SumOfIntegers</id:serviceCode>
          <id:serviceVersion>v1</id:serviceVersion>
        </xrd:service>
        <xrd:id>ABCDE</xrd:id>
        <xrd:userId>30101010001</xrd:userId>
        <xrd:issue>12345</xrd:issue>
        <xrd:protocolVersion>4.0</xrd:protocolVersion>
      </soapenv:Header>
      <soapenv:Body>
        <calc:SumOfIntegersResponse>
          <request>
            <X>12</X>
            <Y>7</Y>
          </request>
          <response>19</response>
        </calc:SumOfIntegersResponse>
      </soapenv:Body>
    </soapenv:Envelope>
