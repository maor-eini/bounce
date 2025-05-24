using HospiSaaS.Domain.ValueObjects;

namespace HospiSaaS.Domain.Entities;

public sealed class OperatingRoom
{
    public Guid Id { get; }
    public string Name { get; }
    public Guid HospitalId { get; }
    public HashSet<Equipment> SetOfEquipment { get; }

    private readonly object _lock = new();
    private readonly List<TimeSlot> _booked = new();

    private OperatingRoom(Guid id, string name, IEnumerable<Equipment> equipment, Guid hospitalId)
    {
        Id = id;
        Name = name;
        HospitalId = hospitalId;
        SetOfEquipment = new HashSet<Equipment>(equipment);
    }

    public static OperatingRoom Create(Guid id, string name, IEnumerable<Equipment> equipment, Guid hospitalId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
        return new OperatingRoom(id, name, equipment, hospitalId);
    }

    public bool CanPerform(SurgeryType type) =>
        type switch
        {
            SurgeryType.Heart => SetOfEquipment.Contains(Equipment.ECG),
            SurgeryType.Brain => SetOfEquipment.Contains(Equipment.MRI),
            _ => false
        };

    public bool TryBook(SurgeryType type, DateTime startUtc)
    {
        var slot = new TimeSlot(startUtc, type.GetDurationHours(this));

        lock (_lock)
        {
            if (_booked.Any(existing => existing.Overlaps(slot)))
                return false;

            _booked.Add(slot);
            return true;
        }
    }
}
