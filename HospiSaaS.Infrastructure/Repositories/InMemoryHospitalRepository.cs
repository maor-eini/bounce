using HospiSaaS.Domain.Entities;
using HospiSaaS.Domain.Repositories;

namespace HospiSaaS.Infrastructure.Repositories;

public class InMemoryHospitalRepository : IHospitalRepository 
{
    private static readonly Dictionary<Guid, Hospital> _hospitals = new();

    public Hospital GetById(Guid hospitalId) {
        _hospitals.TryGetValue(hospitalId, out var hospital);
        return hospital;
    }
    public IEnumerable<Hospital> GetAll() => _hospitals.Values;

    public void Add(Hospital hospital) {
        _hospitals[hospital.Id] = hospital;
    }
    public void Update(Hospital hospital) {
        _hospitals[hospital.Id] = hospital;
    }

    public void Clear() => _hospitals.Clear();
}
