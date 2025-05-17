![Static Badge](https://img.shields.io/badge/MCP%20SDK-preview.14-%239553E9?logo=dotnet)
![Static Badge](https://img.shields.io/badge/MCP%20Endpoints%20Tools-v1.0.6%20alpha-%239553E9?logo=dotnet)

# MCP Endpoints Tools

Library for ASP.NET Core Web API, which automatically turns each controller method into a tool for the MCP
server.

Under the hood, MCP Endpoints Tools uses
the [Model Context Protocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk "Model Context Protocol C# SDK")
for working with tools.

## Description

MCP Endpoints Tools scans the application build, finds all controllers and their public methods marked with HTTP
attributes, and registers them as Model Context Protocol (MCP) tools. In this case, XML comments from the assembly are
used to fill in the description (summary) of the tools.

## Features

* Automatic registration of all controller methods as MCP tools and resources
* Support for method exclusion via the `[McpIgnore]` attribute
* Автоматическое добавление описания инструмента из XML-комментариев сборки через `XmlCommentsProvider`
* Flexible configuration via `ServerOptions` (path, name, description, version, XML path)
* Easy integration into 'IServiceCollection` and `IEndpointRouteBuilder' via extensions `ServiceCollectionExtensions`
  and `EndpointRouteBuilderExtensions`
* An API endpoint is automatically created that returns complete information about the application methods.

## Installation

1. Add the `McpEndpointsTools` project to your solution or connect via NuGet (if there is a package):

   ```bash
   dotnet add package McpEndpointsTools
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
            opts.EndpointOptions.Name = "Endpoints-tools";
            opts.EndpointOptions.Description = "Use this property as an incentive for your LLM. Do not set this property if in doubt. It is better to remove this property from the configuration altogether.";
            opts.EndpointOptions.Title = "Internal tools API";
            opts.EndpointOptions.Endpoint = "/resources";
     });
   ```

   - `opts.EndpointOptions.Endpoint` Method GET returns a paginated list of all endpoints that are available to the MCP
     server. At the same time, it describes in detail the parameters of the methods, including complex ones. You don't
     have
     to specify it in the configuration. An automatic endpoint will be assigned. the full path will look like this:
     `/msp/resources`
   - `opts.EndpointOptions.Description` It is not necessary to use it. Recommended
   - `opts.EndpointOptions.Title` It is not necessary to use it.
   - `opts.EndpointOptions.Name` It is not necessary to use it.

   Example response endpoint `opts.EndpointOptions.Endpoint`
   ```json
   {
     "page": 1,
     "pageSize": 10,
     "totalItems": 4,
     "totalPages": 1,
     "hasPreviousPage": false,
     "hasNextPage": false,
     "items": [
       {
         "name": "weatherforecast-get",
         "description": "Retrieves a collection of weather forecast information for the upcoming days.",
         "uri": "https://api.il2-expert.ru/WeatherForecast",
         "httpMethod": "GET",
         "mimeType": "application/json",
         "params": []
       },
       {
         "name": "weatherforecast-predictrainchance",
         "description": "Predicts the chance of rainfall based on the provided meteorological data.",
         "uri": "https://api.il2-expert.ru/WeatherForecast/PredictRainChance",
         "httpMethod": "POST",
         "mimeType": "application/json",
         "params": [
           {
             "name": "model",
             "type": "object",
             "description": "The data model containing meteorological inputs, such as pressure, used for predicting rainfall.",
             "children": [
               {
                 "name": "Pressure",
                 "type": "number",
                 "description": "Gets or sets the atmospheric pressure value used in predicting the chance of rainfall.\n            This value typically represents the barometric pressure measured in a specific unit, such as hPa or atm.",
                 "children": null
               }
             ]
           }
         ]
       }
     ]
   }
   ```

3. **Enable XML documentation generation** in your `.csproj` project file:
   ```xml
   <PropertyGroup>
     <GenerateDocumentationFile>true</GenerateDocumentationFile>
     <NoWarn>$(NoWarn);1591</NoWarn>
   </PropertyGroup>
   ```

4. Set up routing in the same or another location:

   ```csharp
   var app = builder.Build();

   app.MapControllers(); // regular controllers
   app.MapMcpEndpointsServer(); // MCP endpoints (HTTP stream & SSE)

   app.Run();
   ```
   **Follow the registration procedure**

## Attributes

* `McpIgnoreAttribute` It is placed above the controller method to exclude it from the list of generated MCP tools.
* `Authorize` Methods and controllers marked with the Authorize attribute will not be added to the tools. In future
  versions, support for authentication authorization may be added.

Here is an example of an attribute over a controller method

```csharp
    /// <summary>
    /// Provides clothing advice based on the current temperature in Celsius.
    /// </summary>
    /// <param name="tempC">The temperature in Celsius for which clothing advice is needed.</param>
    /// <returns>
    /// A string containing clothing advice suitable for the specified temperature.
    /// </returns>
    [HttpGet("GetClothingAdvice")]
    [McpIgnore]
    public IActionResult GetClothingAdvice([FromQuery] double tempC)
    {
        if (tempC >= 25) return Ok("Put on a T-shirt and shorts.");
        if (tempC >= 15) return Ok("A light jacket will do.");
        if (tempC >= 5) return Ok("I need a coat.");
        return Ok("It's very cold — keep warm!");
    }
```

If you mark the entire controller with this attribute, all controller methods will be ignored by the MCP server.

### Example app

See the sample application on ASP .NET Core is available via
the [link](https://github.com/DED-Zlodey/McpEndpontsTools/tree/master/WebApiExample "WebApiExample")


### Example config for IDEs

#### VS Code global

   ```json
   {
       "workbench.startupEditor": "none",
       "explorer.confirmDelete": false,
       "mcp": {
           "servers": {
               "test-mcp": {
                   "url": "http://localhost:5220/mcp"
               },
               "API-helper": {
                   "url": "https://api.il2-expert.ru/mcp"
               }
           }
       },
       "explorer.confirmDragAndDrop": false,
       "chat.editing.confirmEditRequestRetry": false
   }
   ```
#### Cursor global
   ```json
   {
       "mcpServers": {
           "my-mcp-server": {
               "url": "http://localhost:5258/mcp",
               "type": "http"
           }
       }
   }
   ```


#### Live demo
The demo API application is deployed at: [https://api.il2-expert.ru/mcp/resources](https://api.il2-expert.ru/mcp/resources "Demo")
To test connecting the MP IDE client to the endpoint: https://api.il2-expert.ru/mcp


## License

MIT License. See the LICENSE file for details.