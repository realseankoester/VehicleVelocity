using System;
using System.Collections.Generic;
using VehicleVelocity.Common.Models;

namespace VehicleVelocity.Common.Services;

public interface IQualityAuditService
{
    // Updated signature: It now accepts the boolean result from the AI Agent
    AuditResult AnalyzeVehicle(Vehicle car, bool hasStructuralDamage);
}

public class AuditResult
{
    public string RiskReason { get; set; } = string.Empty;
    public int QualityScore { get; set; }
    public int PriorityLevel { get; set; } 
}

public class QualityAuditService : IQualityAuditService
{
    public AuditResult AnalyzeVehicle(Vehicle car, bool hasStructuralDamage)
    {
        if (car == null) throw new ArgumentNullException(nameof(car));

        int currentScore = 100;
        var reasons = new List<string>();

        // 1. Piecewise Mileage Penalty (The Carvana Cliff)
        if (car.Mileage <= 120000)
        {
            currentScore -= (car.Mileage / 10000);
        }
        else
        {
            int excessMileage = car.Mileage - 120000;
            currentScore -= (12 + (excessMileage / 3000));
            reasons.Add("High Mileage Range");
        }

        // 2. REPLACED BRITTLE STRING CHECKS WITH SMART AI ENGINE SIGNAL
        // Instead of breaking on phrases like "no rust found", we rely on the LLM's context logic
        if (hasStructuralDamage)
        {
            currentScore -= 50; // Decisive deduction for validated defects
            reasons.Add("CRITICAL DEFECT DETECTED BY LOCAL AI");
        }

        // 3. Score Normalization
        currentScore = Math.Clamp(currentScore, 0, 100);

        // 4. Priority Derivation Logic
        bool isCritical = hasStructuralDamage || (currentScore < 45);
        int finalPriority = isCritical ? 1 : (car.Mileage > 120000 ? 2 : 3);

        return new AuditResult
        {
            QualityScore = currentScore,
            RiskReason = reasons.Count > 0 ? string.Join(", ", reasons) : "Clear",
            PriorityLevel = finalPriority
        };
    }
}