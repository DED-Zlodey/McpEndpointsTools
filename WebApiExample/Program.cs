using System.Reflection;
using McpEndpointsTools.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Logging.AddFilter("ModelContextProtocol.Server", LogLevel.Trace);

builder.Services.AddControllers();

builder.Services.AddMcpEndpointsServer(opts =>
{
    opts.McpEndpoint = "/mcp/";
    opts.ServerName = "API Server Helper";
    opts.ServerDescription = "The server for getting the weather";
    opts.ServerVersion = "1.0.0";
    opts.XmlCommentsPath = Path.Combine(
        AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"
    );
    opts.HostUrl = "http://localhost:5258";
    opts.EndpointOptions.Name = "Endpoints-tools";
    opts.EndpointOptions.Description = "Tools for working with endpoints";
    opts.EndpointOptions.Title = "Internal tools API";
    opts.EndpointOptions.Endpoint = "/resources";
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        bls =>
        {
            bls.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.MapControllers();
app.MapMcpEndpointsServer();

app.Run();