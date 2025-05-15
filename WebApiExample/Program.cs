using System.Reflection;
using McpEndpointsTools.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Logging.AddFilter("ModelContextProtocol.Server", LogLevel.Trace);

builder.Services.AddControllers();

builder.Services.AddMcpEndpointsServer(opts =>
{
    opts.McpEndpoint = "/mcp";
    opts.ServerName = "API Server Helper";
    opts.ServerDescription = "The server for getting the weather";
    opts.ServerVersion = "1.0.0";
    opts.XmlCommentsPath = Path.Combine(
        AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"
    );
    opts.HostUrl = "http://localhost:5258";
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
if (app.Environment.IsDevelopment())
{
    app.MapMcpEndpointsServer();
}

app.Run();