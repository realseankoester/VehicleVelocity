using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VehicleVelocity.Common.Models;

namespace VehicleVelocity.Common.Services
{
    /// <summary>
    /// Service interface for analyzing vehicle integrity and risk factors.
    /// </summary>
    public interface IQualityAuditService
    {
        /// <summary>
        /// Analyzes a vehicle's telemetry and notes to determine quality metrics.
        /// </summary>
        /// <param name="car">The vehicle data to evaluate.</param>
        /// <returns>A task representing the asynchronous audit operation.</return>
        Task<AuditResult> AnalyzeVehicleAsync(Vehicle car);
    }
    /// <summary>
    /// Represents the calculated results of an automated quality assessment.
    /// </summary>
    public class AuditResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the vehicle should be flagged for lead review.
        /// </summary>
        public bool IsHighPriorityAudit { get; set; }
        /// <summary>
        /// Gets or sets the concatenated list of identified risk factors.
        /// </summary>
        public string RiskReason { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the calculated health score (0-100).
        /// </summary>
        public int QualityScore { get; set; }
        /// <summary>
        /// Gets or sets the urgency level: 1 (Critical), 2 (High), 3 (standard).
        /// </summary>
        public int PriorityLevel {get; set; }
    }
    /// <summary>
    /// Implements heuristic-based auditing logic for rapid vehicle intake assessment.
    /// </summary>
    public class QualityAuditService : IQualityAuditService
    {
        /// <summary>
        /// Evaluates vehicle health based on intake telemetry and keyword sentiment.
        /// Processes raw intake data to produce a standardized QualityScore and PriorityLevel.
        /// </summary>
        /// <param name="car">The vehicle instance to be analyzed.</param>
        /// <returns>An AuditResult enriched with processed metrics.</returns>
        public async Task<AuditResult> AnalyzeVehicleAsync(Vehicle car)
        {
            await Task.Delay(100); 

            int currentScore = 100;
            var reasons = new List<string>();
            bool isStructuralFailure = false;

            // 1. Piecewise Mileage Penalty
            if (car.Mileage <= 120000)
            {
                // Low-to-Mid Mileage: 1pt per 10k miles
                currentScore -= (car.Mileage / 10000);
            }
            else
            {
                // High Mileage Cliff: Base 12pt drop + 1pt per 3k miles over 120k
                int excessMileage = car.Mileage - 120000;
                currentScore -= (12 + (excessMileage / 3000));
                reasons.Add("High Mileage Range");
            }

            // 2. Critical Dealbreakers
            if (car.InspectionNotes?.ToLower().Contains("rust") == true)
            {
                currentScore -= 45;
                reasons.Add("STRUCTURAL RUST");
                isStructuralFailure = true;
            }
            
            if (car.InspectionNotes?.ToLower().Contains("frame") == true || 
                car.InspectionNotes?.ToLower().Contains("leak") == true)
            {
                currentScore -= 40;
                reasons.Add("MECHANICAL/FRAME RISK");
                isStructuralFailure = true;
            }

            // 3. Score Normalization
            currentScore = Math.Max(0, currentScore);

            // 4. Priority Logic
            // Only High Priority if it's a structural failure OR the score is critically low.
            bool isHighPriority = isStructuralFailure || (currentScore < 45);

            return new AuditResult
            {
                QualityScore = currentScore,
                IsHighPriorityAudit = isHighPriority,
                RiskReason = reasons.Count > 0 ? string.Join(", ", reasons) : "Clear",
                PriorityLevel = isHighPriority ? 1 : (car.Mileage > 120000 ? 2 : 3)
            };
        }
    }
}