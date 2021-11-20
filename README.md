# OpenTelemetry Demo application

This repository is a suppliment to my Talk around using [OpenTelemetry](https://opentelemetry.io) with .NET.

It can be used as a standalone example of configuring and using OpenTelemetry with .NET, specifically demoing:

* ASP.NET Core automatic instrumentation
* HttpClient instrumentation
* Using Baggage between mulitple services

## Build

The solution required .NET 6.0 which you can download from here: [Download .NET 6](https://dotnet.microsoft.com/download/dotnet/6.0)

From the repository root directory:

```
dotnet build
```

## Running

The solution is separated into 2 services.

### Services

#### Service1

This is to simulate the Endpoint that a user would interact with directly.

#### Service2

This is to simulate an internal backend service that is only called from the edge service.

### Starting

The first thing to do is get a [Jaegar](https://www.jaegertracing.io/) instance up and running to receive the traces.  This repository provides a preconfigured Jaegar instance in the form of a Docker Compose.  You'll need docker installed in order for this to work.

```
docker-compose up -d
```

Next, we'll need the services to be running.

```
cd service1
dotnet run
```

in another terminal window

```
cd service2
dotnet run
```

### Generating the traces

You'll now be able to open a browser or run wget/curl for the service1 URL.

http://localhost:5042

This will generate a basic single service trace with just the basic ASP.NET Core instrumentation.

http://localhost:5042/child

This will generate a single service trace with a child span with delays so that you can see the generated spans.

http://localhost:5042/baggage

This will generate a distributed trace with baggage propagation.

### Viewing Traces

If you open a browser and navigate to the below URL, you can select `OpenTelemetry Demo` from the services dropdown, then click "Find Traces".

http://localhost:16686/
