using HospiSaaS.Domain.ValueObjects;

namespace HospiSaaS.Domain.Entities;

public sealed class Doctor
{
    public Guid Id { get; }
    public string Name { get; }
    public SurgeryType Specialty { get; }
    public Guid HospitalId { get; }

    private Doctor(Guid id, string name, SurgeryType specialty, Guid hospitalId)
    {
        Id = id;
        Name = name;
        Specialty = specialty;
        HospitalId = hospitalId;
    }

    public static Doctor Create(Guid id, string name, SurgeryType specialty, Guid hospitalId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
        return new Doctor(id, name, specialty, hospitalId);
    }
}

