using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
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
    /// Factory for creating controller instances without additional reflection.
    /// </summary>
    private readonly ObjectFactory _controllerFactory;

    /// <summary>
    /// Precompiled delegate used to invoke the controller method.
    /// </summary>
    private readonly Func<object?, object?[], object?> _methodInvoker;

    /// <summary>
    /// Cached parameter metadata for the controller method.
    /// </summary>
    private readonly ParameterInfo[] _methodParams;

    /// <summary>
    /// Precalculated default parameter values.
    /// </summary>
    private readonly object?[] _defaultArgs;
    
    /// <summary>
    /// Represents a factory for creating service scopes, allowing for dependency injection and
    /// resolution of scoped services in the application. This is used to create an isolated service
    /// provider scope for handling requests, ensuring that dependencies with scoped lifetimes are properly managed.
    /// </summary>
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Represents a handler for processing endpoint requests by mapping them to specific controller methods.
    /// Handles the instantiation of controller instances, preparation of method parameters,
    /// and execution of either synchronous or asynchronous controller methods.
    /// </summary>
    public EndpointHandler(IServiceScopeFactory scopeFactory, Type controllerType, MethodInfo controllerMethod)
    {
        _scopeFactory = scopeFactory;
        _controllerFactory = ActivatorUtilities.CreateFactory(controllerType, Type.EmptyTypes);
        _methodParams = controllerMethod.GetParameters();
        _defaultArgs = new object?[_methodParams.Length];
        for (int i = 0; i < _methodParams.Length; i++)
        {
            var p = _methodParams[i];
            _defaultArgs[i] = p.HasDefaultValue ? p.DefaultValue : GetDefault(p.ParameterType);
        }
        _methodInvoker = CreateMethodInvoker(controllerMethod);
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
        var controller = _controllerFactory(sp, null);

        var args = new object?[_methodParams.Length];

        for (int i = 0; i < _methodParams.Length; i++)
        {
            var param = _methodParams[i];
            if (ctx.Params != null && ctx.Params.Arguments != null && ctx.Params.Arguments.TryGetValue(param.Name!, out var json))
            {
                args[i] = json.Deserialize(param.ParameterType);
            }
            else
            {
                args[i] = _defaultArgs[i];
            }
        }
        
        var result = _methodInvoker(controller, args);
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
    /// Builds a delegate that invokes the specified method with an object target and argument array.
    /// </summary>
    /// <param name="method">The method to create an invoker for.</param>
    /// <returns>A delegate that invokes the method without reflection.</returns>
    private static Func<object?, object?[], object?> CreateMethodInvoker(MethodInfo method)
    {
        var targetParam = Expression.Parameter(typeof(object), "target");
        var argsParam = Expression.Parameter(typeof(object[]), "args");

        var paramInfos = method.GetParameters();
        var argExpressions = new Expression[paramInfos.Length];
        for (int i = 0; i < paramInfos.Length; i++)
        {
            var indexExpr = Expression.ArrayIndex(argsParam, Expression.Constant(i));
            var converted = Expression.Convert(indexExpr, paramInfos[i].ParameterType);
            argExpressions[i] = converted;
        }

        Expression? instanceExpr = method.IsStatic ? null : Expression.Convert(targetParam, method.DeclaringType!);
        var callExpr = Expression.Call(instanceExpr, method, argExpressions);
        Expression body = method.ReturnType == typeof(void)
            ? Expression.Block(callExpr, Expression.Constant(null))
            : Expression.Convert(callExpr, typeof(object));

        return Expression.Lambda<Func<object?, object?[], object?>>(body, targetParam, argsParam).Compile();
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