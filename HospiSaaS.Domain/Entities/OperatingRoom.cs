using HospiSaaS.Domain.ValueObjects;

namespace HospiSaaS.Domain.Entities;

public class OperatingRoom {
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public HashSet<SurgeryType> Capabilities { get; private set; }
    public Guid HospitalId { get; private set; }
    
    private readonly HashSet<DateTime> _bookedSlots = [];

    public static OperatingRoom Create(Guid id, string name, IEnumerable<SurgeryType> capabilities, Guid hospitalId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Operating room name is required.");

        if (capabilities == null || !capabilities.Any())
            throw new ArgumentException("At least one capability must be defined.");

        return new OperatingRoom
        {
            Id = id,
            Name = name,
            Capabilities = [..capabilities],
            HospitalId = hospitalId
        };
    }

    public bool CanPerform(SurgeryType type) => Capabilities.Contains(type);
    public bool IsFreeAt(DateTime time) => !_bookedSlots.Contains(time);
    public void Book(DateTime time) => _bookedSlots.Add(time);
}