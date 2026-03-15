using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleVelocity.Common.Models;

namespace VehicleVelocity.Common.Services;

/// <summary>
/// Service for managing the initial intake and validation of vehicle inventory.
/// </summary>
public interface IInventoryService
{
    bool ValidateIntake(Vehicle vehicle, out List<string> errors);
    Task<string> GenerateIntakeBatchIdAsync();
}

public class InventoryService : IInventoryService
{
    /// <summary>
    /// Performs basic validation on a vehicle before it is published to the event stream.
    /// </summary>
    public bool ValidateIntake(Vehicle vehicle, out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(vehicle.Vin) || vehicle.Vin.Length < 11)
            errors.Add("Invalid VIN: Must be at least 11 characters.");

        if (vehicle.Year < 1900 || vehicle.Year > DateTime.Now.Year + 1)
            errors.Add("Invalid Year: Please check the vehicle manufacturing date.");

        if (vehicle.Mileage < 0)
            errors.Add("Invalid Mileage: Value cannot be negative.");

        return errors.Count == 0;
    }

    /// <summary>
    /// Generates a unique tracking ID for the current intake session.
    /// </summary>
    public async Task<string> GenerateIntakeBatchIdAsync()
    {
        await Task.Delay(10); // Simulate light work
        return $"BATCH-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}