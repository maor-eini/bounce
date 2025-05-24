using HospiSaaS.Domain.ValueObjects;

namespace HospiSaaS.Domain.Entities;

public class SurgeryRequest
{
    public Guid Id { get; }
    public Guid DoctorId { get; }
    public SurgeryType Type { get; }
    public DateTime DesiredTimeUtc { get; }
    public RequestStatus Status { get; private set; }
    public Guid? OperatingRoomId { get; private set; }

    private SurgeryRequest(Guid id, Guid doctorId, SurgeryType type,
        DateTime whenUtc, RequestStatus status, Guid? roomId = null)
    {
        Id = id;
        DoctorId = doctorId;
        Type = type;
        DesiredTimeUtc = whenUtc;
        Status = status;
        OperatingRoomId = roomId;
    }

    public static SurgeryRequest Waiting(Guid doctorId, SurgeryType type, DateTime whenUtc)
    {
        return new SurgeryRequest(Guid.NewGuid(), doctorId, type, whenUtc, RequestStatus.Waiting);
    }

    public static SurgeryRequest Scheduled(Guid doctorId, SurgeryType type, DateTime whenUtc, Guid roomId)
    {
        return new SurgeryRequest(Guid.NewGuid(), doctorId, type, whenUtc, RequestStatus.Scheduled, roomId);
    }

    public void MarkScheduled(Guid roomId)
    {
        if (Status == RequestStatus.Scheduled) return;
        Status = RequestStatus.Scheduled;
        OperatingRoomId = roomId;
    }
}