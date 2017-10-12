XRoadLib
======================

XRoadLib is a .NET library for implementing service interfaces of X-Road providers using Code-First Development
approach. Main focus of the library is to support easier service development on X-Road provider side, by extracting
service descriptions and serialization logic from types and methods that are described as service contracts. Main
features the library offers are:

* Base handler for service request, which deserializes request message; calls corresponding method, that implements
  operation logic; and serializes response message to client.
  
* Base handler for service description request, which build WSDL document based on service contract.

* Customizable type, property and method definitions to fine tune message and WSDL appearance.

* Customizable type maps which allow to implement custom logic in type serialization/deserialization process.

The library in currently in beta version, which means it may contain bugs and other implications. It would help, if 
any problem encountered is reported in project [Github][gh] page.

Project is available for use through NuGet package manager:

    PM> Install-Package XRoadLib -Pre

Samples & documentation
-----------------------

The library comes with comprehensible documentation. It can include tutorials automatically generated from `*.fsx`
files in [the content folder][content]. The API reference is automatically generated from Markdown comments in the
library implementation.

 * [Tutorial](tutorial.html) contains a further explanation of this sample library.
 * [.NET Core Tutorial](tutorial-dotnetcore.html) contains example application for .NET Core platform.

 * [API Reference](reference/index.html) contains automatically generated documentation for all types, modules and
   functions in the library. This includes additional brief samples on using most of the functions.

Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork  the project and submit pull
requests. If you're adding a new public API, please also consider adding [samples][content] that can be turned into a
documentation. You might also want to read the [library design notes][readme] to understand how it works.

The library is available under MIT license, which allows modification and redistribution for both commercial and
non-commercial purposes with proper attribution. For more information see the [License file][license] in the GitHub
repository. 

  [content]: https://github.com/janno-p/XRoadLib/tree/master/docs/content
  [gh]: https://github.com/janno-p/XRoadLib
  [issues]: https://github.com/janno-p/XRoadLib/issues
  [readme]: https://github.com/janno-p/XRoadLib/blob/master/README.md
  [license]: https://github.com/janno-p/XRoadLib/blob/master/LICENSE.txt
