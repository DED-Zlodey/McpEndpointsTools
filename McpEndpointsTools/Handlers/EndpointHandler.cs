using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace McpEndpointsTools.Handlers;

/// <summary>
/// The EndpointHandler class is responsible for processing incoming endpoint requests
/// by mapping them to specific methods on a designated controller and executing them.
/// It manages the lifecycle of the controller instance, prepares and passes required
/// method parameters, and handles both synchronous and asynchronous method executions.
/// </summary>
public class EndpointHandler
{
    /// <summary>
    /// Represents a factory for creating service scopes, allowing for dependency injection and
    /// resolution of scoped services in the application. This is used to create an isolated service
    /// provider scope for handling requests, ensuring that dependencies with scoped lifetimes are properly managed.
    /// </summary>
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Represents the type of the controller that defines the method to be invoked for handling a specific endpoint.
    /// This type is used to dynamically create an instance of the controller for the method invocation.
    /// </summary>
    private readonly Type _controllerType;

    /// <summary>
    /// Represents the specific method of a controller that will handle the incoming request.
    /// This method is dynamically invoked during request processing,
    /// based on the runtime configuration of the associated endpoint.
    /// </summary>
    private readonly MethodInfo _controllerMethod;

    /// <summary>
    /// Represents a handler for processing endpoint requests by mapping them to specific controller methods.
    /// Handles the instantiation of controller instances, preparation of method parameters,
    /// and execution of either synchronous or asynchronous controller methods.
    /// </summary>
    public EndpointHandler(IServiceScopeFactory scopeFactory, Type controllerType, MethodInfo controllerMethod)
    {
        _scopeFactory = scopeFactory;
        _controllerType = controllerType;
        _controllerMethod = controllerMethod;
    }

    /// <summary>
    /// Handles the processing of a given request context by invoking the associated controller method.
    /// Constructs the required method parameters and extracts the result from the controller action,
    /// supporting both synchronous and asynchronous methods.
    /// </summary>
    /// <param name="ctx">The request context that includes request parameters and invocation details.</param>
    /// <returns>The result of the controller method execution, deserialized from the action or task result.</returns>
    public object? Handle(RequestContext<CallToolRequestParams> ctx)
    {
        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;
        var controller = ActivatorUtilities.CreateInstance(sp, _controllerType);
        
        var methodParams = _controllerMethod.GetParameters();
        var args = new object?[methodParams.Length];

        for (int i = 0; i < methodParams.Length; i++)
        {
            var param = methodParams[i];
            if (ctx.Params != null && ctx.Params.Arguments != null && ctx.Params.Arguments.TryGetValue(param.Name!, out var json))
            {
                args[i] = json.Deserialize(param.ParameterType);
            }
            else
            {
                args[i] = param.HasDefaultValue ? param.DefaultValue : GetDefault(param.ParameterType);
            }
        }
        
        var result = _controllerMethod.Invoke(controller, args);
        if (result is Task task)
        {
            task.GetAwaiter().GetResult();

            var taskType = task.GetType();
            if (taskType.IsGenericType)
            {
                var innerResult = taskType.GetProperty("Result")!.GetValue(task);
                return innerResult;
            }
            return null;
        }

        return result;
    }

    /// <summary>
    /// Returns the default value for a given type. For value types, it creates a new instance.
    /// For reference types, it returns null.
    /// </summary>
    /// <param name="type">The type for which the default value is to be returned.</param>
    /// <returns>The default value of the specified type, or null for reference types.</returns>
    private static object? GetDefault(Type type)
    {
        if (type.IsValueType)
            return Activator.CreateInstance(type);
        return null;
    }
}