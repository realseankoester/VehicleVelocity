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
        Task<AuditResult> AnalyzeVehicleAsync(Vehicle car);
    }

    public class AuditResult
    {
        public bool NeedsManualReview { get; set; }
        public string RiskReason { get; set; } = string.Empty;
        public int QualityScore { get; set; } // 1-100
        public int PriorityLevel {get; set; } // 1 (Critical) to 3 (Standard)
    }
    
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
            // Placeholder for advanced AI/ML model integration.
            // Currently implements heuristic-based keyword sentiment analysis.
            await Task.Delay(100);

           int currentScore = 100;
           var reasons = new List<string>();

           string notes = car.InspectionNotes?.ToLower() ?? "";

           // Mileage Check
           if (car.Mileage > 120000)
            {
                currentScore -= 30;
                reasons.Add("High Mileage Penalty");
            }

            // Keyword Sentiment Analysis
            if (notes.Contains("rust"))
            {
                currentScore -= 70;
                reasons.Add("Corrosion Detected");
            }

            if (notes.Contains("rip") || notes.Contains("tear"))
            {
                currentScore -= 50;
                reasons.Add("Damage to interior, repair needed");
            }

            if (notes.Contains("dirt") || notes.Contains("residue"))
            {
                currentScore -= 20;
                reasons.Add("Interior or exterior could use cleaning");
            }

            // Calculate Priority Level based on final score
            int priority = currentScore switch
            {
                < 60 => 1, // Critical
                < 85 => 2, // High
                _    => 3  // Standard
            };

            return new AuditResult
            {
                QualityScore = Math.Max(0, currentScore), // Ensure score doesn't go negative
                NeedsManualReview = currentScore < 70,
                RiskReason = reasons.Count > 0 ? string.Join(", ", reasons) : "Clear",
                PriorityLevel = priority
            };
        }
    }
}