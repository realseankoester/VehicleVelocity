using System;
using System.Collections.Generic;
using System.Linq;
using VehicleVelocity.Common.Models;

namespace VehicleVelocity.Common.Services;

/// <summary>
/// Container holding the immutable results of an intake validation pass.
/// </summary>
public record ValidationResult(bool IsValid, IReadOnlyCollection<string> Errors);

public interface IInventoryService
{
    /// <summary>
    /// Performs defensive validation on a vehicle before it is published to Kafka.
    /// </summary>
    ValidationResult ValidateIntake(Vehicle vehicle);
    
    /// <summary>
    /// Generates a unique tracking ID for the current intake session.
    /// </summary>
    string GenerateIntakeBatchId();
}

public class InventoryService : IInventoryService
{
    public ValidationResult ValidateIntake(Vehicle vehicle)
    {
        if (vehicle == null) throw new ArgumentNullException(nameof(vehicle));
        
        var errors = new List<string>();

        // 1. VIN Format Guardrail (11-17 characters standard)
        if (string.IsNullOrWhiteSpace(vehicle.Vin) || vehicle.Vin.Length < 11 || vehicle.Vin.Length > 17)
        {
            errors.Add("Invalid VIN: Must be a standard length between 11 and 17 alphanumeric characters.");
        }

        // 2. Year Boundary Guardrail (Uses UtcNow to avoid localized server clock shifts)
        int currentYear = DateTime.UtcNow.Year;
        if (vehicle.Year < 1900 || vehicle.Year > currentYear + 1)
        {
            errors.Add($"Invalid Year: Vehicle manufacturing year must be between 1900 and {currentYear + 1}.");
        }

        // 3. Telemetry Boundary Guardrail
        if (vehicle.Mileage < 0)
        {
            errors.Add("Invalid Mileage: Odometer values cannot be negative.");
        }

        return new ValidationResult(errors.Count == 0, errors.AsReadOnly());
    }

    public string GenerateIntakeBatchId()
    {
        // Allocation-Free / Cleaner Guid slicing using modern Span/string interpolation patterns
        return $"BATCH-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
    }
}