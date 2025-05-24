using HospiSaaS.Domain.ValueObjects;

namespace HospiSaaS.Application.Dtos
{
    public class SurgeryRequestDto {
        public Guid DoctorId { get; set; }
        public SurgeryType SurgeryType { get; set; }
        public DateTime DesiredTime { get; set; }
    }
}