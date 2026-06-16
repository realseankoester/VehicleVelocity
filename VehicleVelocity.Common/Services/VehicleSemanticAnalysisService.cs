using System;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Serilog;

namespace VehicleVelocity.Common.Services;

public interface IVehicleSemanticAnalysisService
{
    Task<(bool HasStructuralDamage, string ExtractionSummary)> AnalyzeInspectionNotesAsync(string notes);
}

public class VehicleSemanticAnalysisService : IVehicleSemanticAnalysisService
{
    private readonly IChatClient _aiClient;

    public VehicleSemanticAnalysisService(IChatClient aiClient)
    {
        _aiClient = aiClient ?? throw new ArgumentNullException(nameof(aiClient));
    }

    public async Task<(bool HasStructuralDamage, string ExtractionSummary)> AnalyzeInspectionNotesAsync(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes) || notes.Equals("Clean/No notes", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "No active defects reported.");
        }

        string systemPrompt = 
            "You are an expert automotive quality assurance inspector evaluating mechanic field notes for an e-commerce platform. " +
            "Your job is to determine if the text explicitly states active structural frame damage, severe geometric suspension distortion, or un-repaired rust perforation. " +
            "Do NOT trigger flags for historical repairs or parts reported as pristine, clean, or perfect.\n" +
            "Format your output EXACTLY like this with no conversational filler:\n" +
            "DAMAGE_FOUND: [true/false]\n" +
            "SUMMARY: [Brief description of actual issues, or 'Clear']";

        try
        {
            // 1. Invoke the stable response method signature
            var response = await _aiClient.GetResponseAsync(new[]
            {
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, $"Analyze these notes: '{notes}'")
            });

            // 2. Access the text directly via the clean stable property sequence
            string aiText = response.Text ?? string.Empty;

            bool hasDamage = aiText.Contains("DAMAGE_FOUND: true", StringComparison.OrdinalIgnoreCase);
            
            string summary = "Clear";
            int summaryIndex = aiText.IndexOf("SUMMARY:", StringComparison.OrdinalIgnoreCase);
            if (summaryIndex != -1)
            {
                summary = aiText[(summaryIndex + 8)..].Trim();
            }

            return (hasDamage, summary);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Local AI inference failed. Defaulting to safe fallback parameters.");
            return (false, "AI Analysis offline - bypassed structural check.");
        }
    }
}