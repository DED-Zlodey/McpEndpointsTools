# MCP Endpoints Tools

Библиотека для ASP.NET Core Web API, автоматически превращающая каждый метод контроллера в инструмент и ресурс для MCP-сервера.

Под капотом MCP Endpoints Tools использует [Model Context Protocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk "Model Context Protocol C# SDK") для работы с инструментами и ресурсами.

## Описание

MCP Endpoints Server сканирует сборку приложения, находит все контроллеры и их публичные методы, аннотированные HTTP-атрибутами, и регистрирует их как инструменты (tools) и ресурсы (resources) в Model Context Protocol (MCP) сервере. При этом используется XML-комментарии из сборки для заполнения описания (summary) инструментов и ресурсов.

## Возможности

* Автоматическая регистрация всех контроллерных методов как MCP-инструментов и ресурсов
* Поддержка исключения методов через атрибут `[McpIgnore]`
* Загрузка описаний из XML-комментариев сборки через `XmlCommentsProvider`
* Гибкая настройка через `ServerOptions` (путь, имя, описание, версия, путь к XML)
* Простая интеграция в `IServiceCollection` и `IEndpointRouteBuilder` через расширения `ServiceCollectionExtensions` и `EndpointRouteBuilderExtensions`

## Структура репозитория

```plaintext
solution/
├── src/
│   └── McpEndpointsTools/        # исходники библиотеки
│       ├── Extensions/           # расширения для IServiceCollection и IEndpointRouteBuilder
│       ├── Attributes/           # McpIgnoreAttribute
│       ├── Providers/            # XmlCommentsProvider и XmlCommentsNameHelper
│       └── Options/              # конфигурация
├── examples/                     # примеры использования
│   └── SampleApp/                # пример ASP.NET Core Web Api приложения
└── README.md                     
```

## Установка

1. Добавьте проект `McpEndpointsTools` в ваше решение или подключите через NuGet (при наличии пакета):

   ```bash
   dotnet add package McpEndpointsServer
   ```

2. В файле `Program.cs` (или `Startup.cs`) зарегистрируйте сервисы и маппинг:

   ```csharp
   using McpEndpointsServer.Extensions;

   var builder = WebApplication.CreateBuilder(args);

   // Регистрация MCP-сервера и сканирование контроллеров
   builder.Services.AddMcpEndpointsServer(opts =>
   {
       opts.PipelineEndpoint   = "/mcp";               // путь для HTTP-пайплайна
       opts.ServerName         = "My MCP Server";      // имя сервера
       opts.ServerDescription  = "API для MCP";        // описание
       opts.ServerVersion      = "1.2.3";              // версия
       opts.XmlCommentsPath    = "MyApp.xml";          // путь к файлу XML-документации
       opts.HostUrl            = "https://api.mysite"; // базовый URL
   });
   ```

3. В том же или другом месте настройте маршрутизацию:

   ```csharp
   var app = builder.Build();

   app.MapControllers();                           // обычные контроллеры
   app.MapMcpEndpointsServer();                    // MCP-эндоинты (HTTP stream & SSE)

   app.Run();
   ```


## Атрибуты

* `McpIgnoreAttribute`
  Проставляется над методом контроллера для исключения его из списка генерируемых MCP-инструментов.


## Лицензия

MIT License. Смотрите файл LICENSE для деталей.
