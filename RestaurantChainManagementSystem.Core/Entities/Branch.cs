using RestaurantChainManagementSystem.Core.Extensions;

namespace RestaurantChainManagementSystem.Core.Entities;

public sealed class Branch
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string OpeningHours { get; set; } = string.Empty;
    public string ManagerEmployeeId { get; set; } = string.Empty;
    public List<BranchMenuItem> MenuItems { get; set; } = [];
    public List<InventoryItem> InventoryItems { get; set; } = [];

    public Branch()
    {
    }

    private Branch(
        string id,
        string name,
        string address,
        string contactNumber,
        string openingHours,
        string managerEmployeeId)
    {
        Id = id.GuidId(nameof(id));
        Name = name.Required(nameof(name));
        Address = address.Required(nameof(address));
        ContactNumber = contactNumber.ValidPhone(nameof(contactNumber));
        OpeningHours = openingHours.Required(nameof(openingHours));
        ManagerEmployeeId = managerEmployeeId.GuidId(nameof(managerEmployeeId));
    }

    public static Branch Create(
        string id,
        string name,
        string address,
        string contactNumber,
        string openingHours,
        string managerEmployeeId) =>
        new(id, name, address, contactNumber, openingHours, managerEmployeeId);
}
