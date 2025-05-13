# MCP Endpoints Tools

Library for ASP.NET Core Web API, which automatically turns each controller method into a tool and resource for the MCP server.

Under the hood, MCP Endpoints Tools uses the [Model Context Protocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk "Model Context Protocol C# SDK") for working with tools and resources.

## Description

The MCP Endpoints Server scans the application build, finds all controllers and their public methods annotated with HTTP attributes, and registers them as tools and resources on the Model Context Protocol (MCP) server. In this case, XML comments from the assembly are used to fill in the description (summary) of tools and resources.

## Features

* Automatic registration of all controller methods as MCP tools and resources
* Support for method exclusion via the `[McpIgnore]` attribute
* Loading descriptions from XML comments of an assembly via the 'XmlCommentsProvider`
* Flexible configuration via `ServerOptions` (path, name, description, version, XML path)
* Easy integration into 'IServiceCollection` and `IEndpointRouteBuilder' via extensions `ServiceCollectionExtensions` and `EndpointRouteBuilderExtensions`

## Repository structure

```plaintext
solution/
├── src/
│   └── McpEndpointsTools/        # library sources
│       ├── Extensions/           # extensions for IServiceCollection and IEndpointRouteBuilder
│       ├── Attributes/           # McpIgnoreAttribute
│       ├── Providers/            # XmlCommentsProvider and XmlCommentsNameHelper
│       └── Options/              # configuration
├── examples/                     # usage examples
,── SampleApp/ # example ASP.NET Core Web Api of the application
└── README.md                     
```

## Installation

1. Add the `McpEndpointsTools` project to your solution or connect via NuGet (if there is a package):

   ```bash
   dotnet add package McpEndpointsServer
   ```

2. In the file `Program.cs` (or `Startup.cs`), register the services and mapping:

   ```csharp
   using McpEndpointsServer.Extensions;

   var builder = WebApplication.CreateBuilder(args);

   // MCP Server registration and controller scanning
   builder.Services.AddMcpEndpointsServer(opts =>
   {
   opts.PipelineEndpoint = "/mcp"; // path for the HTTP pipeline
          opts.ServerName = "My MCP Server"; // server name
          opts.ServerDescription = "API for MCP"; // description
          opts.ServerVersion = "1.2.3"; // version
          opts.XmlCommentsPath = "MyApp.xml "; // path to the XML documentation file
          opts.HostUrl = "https://api.mysite "; // base URL
   });
   ```

3. Set up routing in the same or another location:

   ```csharp
   var app = builder.Build();

   app.MapControllers(); // regular controllers
   app.MapMcpEndpointsServer(); // MCP endpoints (HTTP stream & SSE)

   app.Run();
   ```


## Attributes

* `McpIgnoreAttribute`
  It is placed above the controller method to exclude it from the list of generated MCP tools.


## License

MIT License. See the LICENSE file for details.
