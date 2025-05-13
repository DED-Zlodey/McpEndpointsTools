using McpEndpointsTools.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace WebApiExample.Controllers;

/// <summary>
/// Represents a controller for handling API endpoints related to body measurements and their analysis.
/// </summary>
[ApiController]
[Route("[controller]")]
[McpIgnore]
public class BodyController : ControllerBase
{
    /// <summary>
    /// An instance of <see cref="ILogger{BodyController}"/> used for logging informational, warning,
    /// error, or debug messages within the <see cref="BodyController"/> class.
    /// </summary>
    private readonly ILogger<BodyController> _logger;

    /// <summary>
    /// A controller responsible for handling requests related to body measurements and their analysis.
    /// </summary>
    public BodyController(ILogger<BodyController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Determines the BMI category based on the provided BMI value.
    /// </summary>
    /// <param name="bmi">The body mass index (BMI) value.</param>
    /// <returns>
    /// Returns a string indicating the BMI category:
    /// "Недостаточный вес" if BMI is less than 18.5,
    /// "Нормальный вес" if BMI is between 18.5 and 24.9,
    /// "Избыточный вес" if BMI is between 25 and 29.9,
    /// "Ожирение" if BMI is 30 or higher.
    /// </returns>
    [HttpGet("GetBMICategory")]
    public ActionResult GetBmiCategory([FromQuery]double bmi)
    {
        if (bmi < 18.5) return Ok("Underweight");
        if (bmi < 25) return Ok("Normal weight");
        if (bmi < 30) return Ok("Overweight");
        return Ok("Fatness");
    }
}