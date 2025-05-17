using Microsoft.AspNetCore.Http;

namespace McpEndpointsTools.Infrastructure;

/// Represents a handler for managing requests related to tools endpoints.
/// The ToolsEndpointHandler class is responsible for handling requests
/// that retrieve a paginated list of internal system resources.
public class ToolsEndpointHandler(OperationRegistrar registrar)
{
    /// Handles a paginated request to retrieve a list of internal resources.
    /// <param name="page">The page number of the resources to be retrieved. Defaults to 1 if not specified.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 10 if not specified.</param>
    /// <returns>An instance of <see cref="IResult"/> containing pagination details and the requested resources.</returns>
    public IResult Handle(int page = 1, int pageSize = 10)
    {
        var resources = registrar.GetInternalResources();
        var totalItems = resources.Count;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        
        if (totalItems == 0)
        {
            return Results.Ok(new
            {
                page,
                pageSize,
                totalItems = 0,
                totalPages = 0,
                hasPreviousPage = false,
                hasNextPage = false,
                items = Array.Empty<object>()
            });
        }

        page = Math.Clamp(page, 1, totalPages);

        var items = resources
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
            

        var response = new
        {
            page,
            pageSize,
            totalItems,
            totalPages,
            hasPreviousPage = page > 1,
            hasNextPage = page < totalPages,
            items
        };

        return Results.Ok(response);
    }
}