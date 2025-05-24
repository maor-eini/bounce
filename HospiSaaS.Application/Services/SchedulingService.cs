using HospiSaaS.Domain.Entities;
using HospiSaaS.Domain.Exceptions;
using HospiSaaS.Domain.Repositories;
using SurgeryType = HospiSaaS.Domain.ValueObjects.SurgeryType;

namespace HospiSaaS.Application.Services
{
    public class SchedulingService 
    {
        private readonly IHospitalRepository _hospitalRepo;
        public SchedulingService(IHospitalRepository hospitalRepo) 
        {
            _hospitalRepo = hospitalRepo;
        }

        public SurgeryRequest RequestSurgery(Guid hospitalId, Guid doctorId, SurgeryType type, DateTime dateTime) 
        {
            var hospital = _hospitalRepo.GetById(hospitalId);
            if (hospital == null) {
                throw new KeyNotFoundException($"Hospital {hospitalId} not found");
            }
            
            try {
                // Delegate to domain logic
                var request = hospital.ScheduleSurgery(doctorId, type, dateTime);
                _hospitalRepo.Update(hospital); 
                return request;
            }
            catch (DomainException ex) {
                // Forward domain exception (could wrap in another exception type or handle differently)
                throw;
            }
        }
        
        public void ProcessHospitalQueue(Guid hospitalId) 
        {
            var hospital = _hospitalRepo.GetById(hospitalId);
            hospital.ProcessPendingRequests();
            _hospitalRepo.Update(hospital);
        }
    }
}