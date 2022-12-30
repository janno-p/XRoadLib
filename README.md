# XRoadLib

A .NET library for implementing service interfaces of [X-Road](http://x-road.eu) providers using Code-First Development approach.

[![NuGet](https://buildstats.info/nuget/XRoadLib)](https://www.nuget.org/packages/XRoadLib/)  
[![Build Status](https://github.com/janno-p/XRoadLib/actions/workflows/general.yml/badge.svg?branch=main&event=push)](https://github.com/janno-p/XRoadLib/actions/workflows/general.yml)

## Documentation

Documentation and samples can be found at the [XRoadLib home page](http://janno-p.github.io/XRoadLib/).

Generating documents requires DocFX to be installed:

```cmd
choco install docfx
```

## Prerequisites

* Restore 3rd party dependencies:

  **Mono**: Run `$ mono ./.paket/paket.exe restore`  
  **Windows**: Run `> .paket\paket.exe restore`

* Install FAKE dotnet SDK global tool:

  ```sh
  dotnet tool install fake-cli -g
  ```

https://fsprojects.github.io/Paket/fsi-integration.html

## Building

```sh
fake run build.fsx
```

## Disclaimer

This is an alpha build and as such most likely has problems that are yet undetected. That means the solution is not suitable
for use in production environment. I will not hold responsibility for any damage caused by this software.

## Maintainer(s)

* [@janno-p](https://github.com/janno-p)
