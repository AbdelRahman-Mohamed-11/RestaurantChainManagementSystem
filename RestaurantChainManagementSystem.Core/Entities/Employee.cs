using RestaurantChainManagementSystem.Core.Enums;
using RestaurantChainManagementSystem.Core.Extensions;

namespace RestaurantChainManagementSystem.Core.Entities;

public sealed class Employee
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public EmployeePosition Position { get; set; }
    public decimal Salary { get; set; }
    public DateOnly HireDate { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PrimaryBranchId { get; set; } = string.Empty;
    public DeliveryProfile? DeliveryProfile { get; set; }

    public Employee()
    {
    }

    private Employee(
        string id,
        string fullName,
        EmployeePosition position,
        decimal salary,
        DateOnly hireDate,
        string phoneNumber,
        string email,
        string primaryBranchId)
    {
        Id = id.GuidId(nameof(id));
        FullName = fullName.Required(nameof(fullName));
        Position = position;
        Salary = salary.Positive(nameof(salary));
        HireDate = hireDate.NotFuture(nameof(hireDate));
        PhoneNumber = phoneNumber.ValidPhone(nameof(phoneNumber));
        Email = email.ValidEmail(nameof(email));
        PrimaryBranchId = primaryBranchId.GuidId(nameof(primaryBranchId));
    }

    public static Employee Create(
        string id,
        string fullName,
        EmployeePosition position,
        decimal salary,
        DateOnly hireDate,
        string phoneNumber,
        string email,
        string primaryBranchId) =>
        new(id, fullName, position, salary, hireDate, phoneNumber, email, primaryBranchId);

    public void AttachDeliveryProfile(DeliveryProfile profile)
    {
        if (Position != EmployeePosition.DeliveryStaff)
        {
            throw new InvalidOperationException("Only delivery staff can have a delivery profile.");
        }

        DeliveryProfile = profile ?? throw new ArgumentNullException(nameof(profile));
    }
}
