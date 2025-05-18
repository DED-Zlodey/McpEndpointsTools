# WebApiExample

## Overview
WebApiExample is a demonstration ASP.NET Core Web API project that provides weather forecasting functionality. This API allows users to retrieve weather forecast data and calculate wind speed using specific parameters. Additionally, it offers endpoints for BMI category determination and provides clothing recommendations based on temperature.

## Features
- Weather forecast retrieval for upcoming days
- Wind speed calculation based on logarithmic wind profile theory
- Swagger UI for API documentation and testing
- MCP Endpoints Tools integration for development environment

## Technical Details
- Built with ASP.NET Core and .NET 8.0
- RESTful API architecture
- XML documentation for API endpoints
- Swagger/OpenAPI support
- McpEndpointsTools support

## API Endpoints

### GET /WeatherForecast
Returns a 5-day weather forecast with random temperature values and weather conditions.

### GET /WeatherForecast/GetWindSpeed
Calculates wind speed at a specific height above ground level based on provided parameters:
- `v`: Initial wind speed
- `h`: Initial height (in meters)
- `k`: The coefficient of the underlying surface of the earth (usually it is from 0.12 to 0.15)

### POST /WeatherForecast/PredictRainChance
Predicts the probability of rain based on atmospheric pressure:
- Request body: A floating-point value representing atmospheric pressure in hPa/millibars
- Returns a message indicating the likelihood of precipitation based on the pressure value

### GET /WeatherForecast/GetClothingAdvice
Provides clothing recommendations based on the current temperature:
- `tempC`: The temperature in degrees Celsius
- Returns appropriate clothing advice for the specified temperature range

### GET /Body/GetBMICategory
Determines the BMI (Body Mass Index) category based on the provided BMI value:
- `bmi`: The Body Mass Index value as a double
- Returns a category description: "Underweight", "Normal weight", "Overweight", or "Fatness"


## Configuration
The application includes various configuration options through the MCP Endpoints Server:
- Server Name: "API Server Helper"
- Server Description: "The server for getting the weather"
- Server Version: "1.0.0"
- MCP Endpoint: "/mcp" (available in development environment only)

## McpEndpointsTools Integration

This project demonstrates the integration of the McpEndpointsTools library with ASP.NET Core Web API. The library provides functionality for creating and exposing API endpoints through the Model Context Protocol (MCP).

### Installation and Library Setup

1. **Add the McpEndpointsTools library to your project** using NuGet Package Manager:
   ```
   dotnet add package McpEndpointsTools
   ```

2. **Enable XML documentation generation** in your `.csproj` project file:
   ```xml
   <PropertyGroup>
     <GenerateDocumentationFile>true</GenerateDocumentationFile>
     <NoWarn>$(NoWarn);1591</NoWarn>
   </PropertyGroup>
   ```

### Configuring Services and Middleware

In the `Program.cs` file, you need to:

1. **Add the necessary using directives**:
   ```csharp
   using System.Reflection;
   using McpEndpointsTools.Extensions;
   ```

2. **Configure the MCP Endpoints Server** in the service configuration method:
   ```csharp
   builder.Services.AddMcpEndpointsServer(opts =>
   {
       opts.McpEndpoint = "/mcp";                  // MCP endpoint path
       opts.ServerName = "API Server Helper";      // Server name
       opts.ServerDescription = "The server for getting the weather"; // Description
       opts.ServerVersion = "1.0.0";               // Version
       
       // Important! Specify the path to the XML documentation file
       opts.XmlCommentsPath = Path.Combine(
           AppContext.BaseDirectory,
           $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"
       );
       
       opts.HostUrl = "http://localhost:5258";     // Host URL
   });
   ```



3. **Configure Swagger to use XML documentation**:
   ```csharp
   builder.Services.AddSwaggerGen(options => 
       options.IncludeXmlComments(Path.Combine(
           AppContext.BaseDirectory,
           $"{Assembly.GetExecutingAssembly().GetName().Name}.xml")));
   ```

4. **Add MCP middleware to the HTTP request pipeline**:
   ```csharp
   // Configure the HTTP request pipeline
   var app = builder.Build();
   
   // Other middleware...
   
   // Register controllers
   app.MapControllers();
   
   // Connect MCP Endpoints Server (only in development mode)
   if (app.Environment.IsDevelopment())
   {
       app.MapMcpEndpointsServer();
   }
   
   app.Run();
   ```

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
   - The demo API application is deployed at: [https://api.il2-expert.ru/mcp/resources](https://api.il2-expert.ru/mcp/resources "Demo")
   - To test connecting the MCP IDE client to the endpoint: https://api.il2-expert.ru/mcp


###



