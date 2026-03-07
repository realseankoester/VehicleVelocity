namespace VehicleVelocity.Common.Services;

public interface IImageAnalysisService
{
    // Returns a string describing detected damage or "Clear"
    Task<string> AnalyzeImageAsync(string ImageUrl);
}