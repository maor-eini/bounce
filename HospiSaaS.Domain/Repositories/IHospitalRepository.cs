using HospiSaaS.Domain.Entities;

namespace HospiSaaS.Domain.Repositories;

public interface IHospitalRepository 
{
    Hospital GetById(Guid hospitalId);
    IEnumerable<Hospital> GetAll();
    void Add(Hospital hospital);
    void Update(Hospital hospital);
}