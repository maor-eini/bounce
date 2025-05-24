using HospiSaaS.Domain.Entities;
using HospiSaaS.Domain.Repositories;
using HospiSaaS.Domain.ValueObjects;

namespace HospiSaaS.Infrastructure.Data;


public static class SeedConstants
{
    public static readonly Guid HospitalId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid DoctorMosheId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"); // Heart
    public static readonly Guid DoctorYitzhakId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"); // BRain
    public static readonly Guid DoctorMayaId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"); // Heart
    public static readonly Guid DoctorLilyId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"); // Heart

}

public static class SampleDataSeeder
{
    public static void Seed(IHospitalRepository repo)
    {
        var hospital = Hospital.Create(SeedConstants.HospitalId, "General Hospital");

        var doctorA = Doctor.Create(SeedConstants.DoctorMosheId, "Dr. Moshe", SurgeryType.Heart, SeedConstants.HospitalId);
        var doctorB = Doctor.Create(SeedConstants.DoctorYitzhakId, "Dr. Yitzhak", SurgeryType.Brain, SeedConstants.HospitalId);
        var doctorC = Doctor.Create(SeedConstants.DoctorMayaId, "Dr. Maya", SurgeryType.Heart, SeedConstants.HospitalId);
        var doctorD = Doctor.Create(SeedConstants.DoctorLilyId, "Dr. Lily", SurgeryType.Heart, SeedConstants.HospitalId);

        var or1 = OperatingRoom.Create(Guid.NewGuid(), "OR-1", new[] { SurgeryType.Heart }, SeedConstants.HospitalId);
        var or2 = OperatingRoom.Create(Guid.NewGuid(), "OR-2", new[] { SurgeryType.Heart }, SeedConstants.HospitalId);
        var or3 = OperatingRoom.Create(Guid.NewGuid(), "OR-3", new[] { SurgeryType.Brain }, SeedConstants.HospitalId);
        
        hospital.AddDoctor(doctorA);
        hospital.AddDoctor(doctorB);
        hospital.AddDoctor(doctorC);
        hospital.AddDoctor(doctorD);
        hospital.AddOperatingRoom(or1);
        hospital.AddOperatingRoom(or2);
        hospital.AddOperatingRoom(or3);

        repo.Add(hospital);
    }
}