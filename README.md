# XRoadLib

A .NET library for implementing service interfaces of [X-Road](http://x-road.eu) providers using Code-First Development approach.

[![NuGet](https://buildstats.info/nuget/XRoadLib?includePreReleases=true)](https://www.nuget.org/packages/XRoadLib/)

## Build Status

[![Mono build status](https://img.shields.io/travis/janno-p/XRoadLib/master.svg?label=Mono%20build)](https://travis-ci.org/janno-p/XRoadLib/)  
[![Windows build status](https://img.shields.io/appveyor/ci/janno-p/xroadlib/master.svg?label=Windows%20build)](https://ci.appveyor.com/project/janno-p/xroadlib)

## Documentation

Documentation and samples can be found at the [XRoadLib home page](http://janno-p.github.io/XRoadLib/).

## Prerequisites

* Restore 3rd party dependencies:

  **Mono**: Run `$ mono ./.paket/paket.exe restore`  
  **Windows**: Run `> .paket\paket.exe restore`

* Install FAKE dotnet SDK global tool:

  ```sh
  dotnet tool install fake-cli -g
  ```

## Building

```sh
fake run build.fsx
```

## Disclaimer

This is an alpha build and as such most likely has problems that are yet undetected. That means the solution is not suitable
for use in production environment. I will not hold responsibility for any damage caused by this software.

## Maintainer(s)

* [@janno-p](https://github.com/janno-p)
