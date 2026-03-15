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
            // Simulation of async AI processing delay.
            await Task.Delay(100);

           int currentScore = 100;
           var reasons = new List<string>();

           string notes = car.InspectionNotes?.ToLower() ?? "";

           // --- 1. Rule: Mileage Check ---
           if (car.Mileage > 120000)
            {
                currentScore -= 30;
                reasons.Add("High Mileage Penalty");
            }

            // --- 2. Rule: Structural & Cosmetic Integrity ---
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

            // --- 3. Priority Mapping ---
            // 1 = Critical (Immediate Lead Specialist Attention)
            // 2 = High (Scheduled Specialist Review)
            // 3 = Standard (Automated Path)
            int priority = currentScore switch
            {
                < 60 => 1, // Critical
                < 85 => 2, // High
                _    => 3  // Standard
            };

            return new AuditResult
            {
                QualityScore = Math.Max(0, currentScore), // Ensure score doesn't go negative
                IsHighPriorityAudit = currentScore < 70,
                RiskReason = reasons.Count > 0 ? string.Join(", ", reasons) : "Clear",
                PriorityLevel = priority
            };
        }
    }
}