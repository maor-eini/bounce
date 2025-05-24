using HospiSaaS.Domain.ValueObjects;

namespace HospiSaaS.Domain.Entities;

public class Doctor 
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public SurgeryType Specialty { get; private set; }
    public Guid HospitalId { get; private set; }
    
    public static Doctor Create(Guid id, string name, SurgeryType specialty, Guid hospitalId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Doctor name is required.");
        
        return new Doctor
        {
            Id = id,
            Name = name,
            Specialty = specialty,
            HospitalId = hospitalId
        };
    }
}
