namespace WebApiExample.Models.RequestModels;

/// <summary>
/// Represents a data model used for predicting the chance of rainfall based on meteorological inputs.
/// </summary>
public class PredictChanceRainfallModel
{
    /// <summary>
    /// Gets or sets the atmospheric pressure value used in predicting the chance of rainfall.
    /// This value typically represents the barometric pressure measured in a specific unit, such as hPa or atm.
    /// </summary>
    public double Pressure { get; set; }
}