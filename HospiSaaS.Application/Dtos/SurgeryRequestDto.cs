using HospiSaaS.Domain.ValueObjects;

namespace HospiSaaS.Application.Dtos
{
    public record SurgeryRequestDto(Guid DoctorId, SurgeryType SurgeryType, DateTime DesiredTimeUtc);

}