using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VehicleVelocity.Common.Models;

namespace VehicleVelocity.Common.Services
{
    public interface IQualityAuditService
    {
        Task<AuditResult> AnalyzeVehicleAsync(Vehicle car);
    }

    public class AuditResult
    {
        public bool NeedsManualReview { get; set; }
        public string RiskReason { get; set; }
        public int QualityScore { get; set; } // 1-100
    }

    public class QualityAuditService : IQualityAuditService
    {
        public async Task<AuditResult> AnalyzeVehicleAsync(Vehicle car)
        {
            // This is where a call to Azure OpenAI or a machine learning model would happen.
            // For demo, use "Keyword Sentiment Analysis" to simulate AI.

            await Task.Delay(100); // Simulate network latency to an AI API

           int currentScore = 100;
           var reasons = new List<string>();

           string notes = car.InspectionNotes?.ToLower() ?? "";

           // Mileage Check
           if (car.Mileage > 120000)
            {
                currentScore -= 30;
                reasons.Add("High Mileage Penalty");
            }

            // Keyword Search
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

            return new AuditResult
            {
                QualityScore = currentScore,
                NeedsManualReview = currentScore < 70, // Any score below 70 flags for manual review
                RiskReason = string.Join(", ", reasons)
            };
        }
    }
}