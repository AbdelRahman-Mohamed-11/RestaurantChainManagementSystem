using RestaurantChainManagementSystem.Core.Extensions;

namespace RestaurantChainManagementSystem.Core.Entities;

public sealed class DeliveryProfile
{
    public string VehicleType { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string AssignedArea { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }

    public DeliveryProfile()
    {
    }

    private DeliveryProfile(string vehicleType, string licenseNumber, string assignedArea)
    {
        VehicleType = vehicleType.Required(nameof(vehicleType));
        LicenseNumber = licenseNumber.Required(nameof(licenseNumber));
        AssignedArea = assignedArea.Required(nameof(assignedArea));
        IsAvailable = true;
    }

    public static DeliveryProfile Create(string vehicleType, string licenseNumber, string assignedArea) =>
        new(vehicleType, licenseNumber, assignedArea);

    public void MarkAssigned() => IsAvailable = false;

    public void MarkAvailable() => IsAvailable = true;
}
