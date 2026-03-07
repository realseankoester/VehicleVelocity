namespace VehicleVelocity.Common.Services;

public class ImageAnalysisService : IImageAnalysisService
{
    public async Task<string> AnalyzeImageAsync(string ImageUrl)
    {
        // Simulate the "Processing" time of an AI model
        await Task.Delay(100);

        // Mock Logic: We "detect" issues baded on keywords in the URL
        if (string.IsNullOrEmpty(ImageUrl)) return "No image provided for analysis.";

        var url = ImageUrl.ToLower();

        if (url.Contains("dent")) return "AI detected minor body damage (Dent).";
        if (url.Contains("crack")) return "AI detected windshield compromise (Crack).";
        if (url.Contains("rust")) return "AI detected significant oxidation (Rust).";

        return "AI analysis: Visual condition appears optimal.";
    }
}