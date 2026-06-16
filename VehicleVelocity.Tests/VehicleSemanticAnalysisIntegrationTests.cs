using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using OllamaSharp;
using VehicleVelocity.Common.Services;

namespace VehicleVelocity.Tests;

public class VehicleSemanticAnalysisIntegrationTests
{
    private readonly IVehicleSemanticAnalysisService _aiAnalysisService;

    public VehicleSemanticAnalysisIntegrationTests()
    {
        // Setup a mirror of your production DI chain pointing to your local Ollama node
        // NOTE: Ensure your local Ollama server is running ('ollama run llama3.2') before starting this test!
        IChatClient testOllamaClient = new OllamaApiClient(new Uri("http://localhost:11434/"), "llama3.2");
        _aiAnalysisService = new VehicleSemanticAnalysisService(testOllamaClient);
    }

    // ==========================================
    // TEST 1: CONFIRM LLM SPOTS SEVERE STRUCTURAL REASONING
    // ==========================================
    [Fact]
    public async Task AnalyzeInspectionNotesAsync_WithExplicitStructuralDamage_ShouldReturnTrueAndSummary()
    {
        // 1. Arrange
        string toxicNotes = "Inspector notes: Car looks okay from outside, but upon lifting, discovered heavy structural rust perforation along the driver side unibody frame rail. Major failure threat.";

        // 2. Act
        var result = await _aiAnalysisService.AnalyzeInspectionNotesAsync(toxicNotes);

        // 3. Assert
        result.HasStructuralDamage.Should().BeTrue("because unibody frame rail rust perforation is a severe structural hazard");
        result.ExtractionSummary.Should().NotBeNullOrWhiteSpace();
        result.ExtractionSummary.Should().NotContain("Clear", "because the AI should have captured the frame details");
    }

    // ==========================================
    // TEST 2: CONFIRM LLM IGNORES MINOR COSMETIC OR MECHANICAL ISSUES
    // ==========================================
    [Fact]
    public async Task AnalyzeInspectionNotesAsync_WithCosmeticAndMechanicalOnlyIssues_ShouldReturnFalse()
    {
        // 1. Arrange
        string cleanNotes = "Rear bumper cover has scratch from low-speed backing incident. Engine runs great but needs new spark plugs. Suspension geometry and frame elements are straight and pristine.";

        // 2. Act
        var result = await _aiAnalysisService.AnalyzeInspectionNotesAsync(cleanNotes);

        // 3. Assert
        result.HasStructuralDamage.Should().BeFalse("because cosmetic scratches and spark plugs do not impact vehicle frame integrity");
        result.ExtractionSummary.Should().NotBeNullOrWhiteSpace();
    }

    // ==========================================
    // TEST 3: VERIFY SHORT-CIRCUIT TRIVIAL NOTES RULE
    // ==========================================
    [Fact]
    public async Task AnalyzeInspectionNotesAsync_WithCleanDefaultString_ShouldShortCircuitWithoutCompute()
    {
        // 1. Arrange
        string cleanNotes = "Clean/No notes";

        // 2. Act
        var result = await _aiAnalysisService.AnalyzeInspectionNotesAsync(cleanNotes);

        // 3. Assert
        result.HasStructuralDamage.Should().BeFalse();
        result.ExtractionSummary.Should().Be("No active defects reported.");
    }
}