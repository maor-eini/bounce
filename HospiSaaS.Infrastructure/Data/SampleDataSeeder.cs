using HospiSaaS.Domain.Entities;
using HospiSaaS.Domain.Repositories;
using HospiSaaS.Domain.ValueObjects;

namespace HospiSaaS.Infrastructure.Data;


public static class SeedConstants
{
    public static readonly Guid HospitalId      = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid DoctorMosheId   = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid DoctorYitzhakId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public static readonly Guid DoctorMayaId    = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    public static readonly Guid DoctorLilyId    = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
}

public static class SampleDataSeeder
{
    public static void Seed(IHospitalRepository repo)
    {
        var hId      = SeedConstants.HospitalId;
        var hospital = Hospital.Create(hId, "Spec-Compliant General Hospital");

        hospital.AddDoctor(Doctor.Create(SeedConstants.DoctorMosheId,   "Dr. Moshe",   SurgeryType.Heart,  hId));
        hospital.AddDoctor(Doctor.Create(SeedConstants.DoctorYitzhakId, "Dr. Yitzhak", SurgeryType.Brain,  hId));
        hospital.AddDoctor(Doctor.Create(SeedConstants.DoctorMayaId,    "Dr. Maya",    SurgeryType.Heart,  hId));
        hospital.AddDoctor(Doctor.Create(SeedConstants.DoctorLilyId,    "Dr. Lily",    SurgeryType.Heart,  hId));

        hospital.AddOperatingRoom(OperatingRoom.Create(Guid.NewGuid(), "OR-1",
            [Equipment.MRI, Equipment.CT, Equipment.ECG], hId));

        hospital.AddOperatingRoom(OperatingRoom.Create(Guid.NewGuid(), "OR-2",
            [Equipment.MRI, Equipment.CT], hId));
        
        hospital.AddOperatingRoom(OperatingRoom.Create(Guid.NewGuid(), "OR-3",
            [Equipment.MRI, Equipment.CT], hId));

        hospital.AddOperatingRoom(OperatingRoom.Create(Guid.NewGuid(), "OR-4",
            [Equipment.MRI, Equipment.ECG], hId));
        
        hospital.AddOperatingRoom(OperatingRoom.Create(Guid.NewGuid(), "OR-5",
            [Equipment.MRI, Equipment.ECG], hId));

        repo.Add(hospital);
    }
}