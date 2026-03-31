using RestaurantChainManagementSystem.Core.Extensions;

namespace RestaurantChainManagementSystem.Core.Entities;

public sealed class ShiftSchedule
{
    public string Id { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public ShiftSchedule()
    {
    }

    private ShiftSchedule(
        string id,
        string employeeId,
        string branchId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        if (endTime <= startTime)
        {
            throw new InvalidOperationException("Shift end time must be after start time.");
        }

        Id = id.GuidId(nameof(id));
        EmployeeId = employeeId.GuidId(nameof(employeeId));
        BranchId = branchId.GuidId(nameof(branchId));
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
    }

    public static ShiftSchedule Create(
        string id,
        string employeeId,
        string branchId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime) =>
        new(id, employeeId, branchId, date, startTime, endTime);

    public bool Overlaps(ShiftSchedule other) =>
        EmployeeId.Equals(other.EmployeeId, StringComparison.OrdinalIgnoreCase) &&
        Date == other.Date &&
        StartTime < other.EndTime &&
        other.StartTime < EndTime;
}
