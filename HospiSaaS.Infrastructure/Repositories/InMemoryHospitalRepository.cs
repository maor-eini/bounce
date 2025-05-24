using System.Collections.Concurrent;
using HospiSaaS.Domain.Entities;
using HospiSaaS.Domain.Repositories;

namespace HospiSaaS.Infrastructure.Repositories;

public sealed class InMemoryHospitalRepository : IHospitalRepository
{
    private readonly ConcurrentDictionary<Guid, Hospital> _store = new();

    public Hospital? GetById(Guid id) =>
        _store.GetValueOrDefault(id);

    public IEnumerable<Hospital> GetAll() => _store.Values;

    public void Add(Hospital hospital)    => _store[hospital.Id] = hospital;

    public void Update(Hospital hospital) => _store[hospital.Id] = hospital;
    
    public void Clear() => _store.Clear();
}
