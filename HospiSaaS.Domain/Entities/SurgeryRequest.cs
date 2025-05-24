using HospiSaaS.Domain.ValueObjects;

namespace HospiSaaS.Domain.Entities;

public enum SurgeryStatus { Pending, Scheduled }

public class SurgeryRequest {
    public Guid Id { get; private set; }
    public Guid DoctorId { get; private set; }
    public SurgeryType Type { get; private set; }
    public DateTime ScheduledTime { get; private set; }
    public Guid? OperatingRoomId { get; private set; }
    public SurgeryStatus Status { get; private set; }
    
    internal SurgeryRequest(Guid id, Guid doctorId, SurgeryType type, DateTime time, SurgeryStatus status, Guid? roomId = null) {
        Id = id;
        DoctorId = doctorId;
        Type = type;
        ScheduledTime = time;
        Status = status;
        OperatingRoomId = roomId;
    }

    internal void MarkScheduled(Guid roomId) {
        Status = SurgeryStatus.Scheduled;
        OperatingRoomId = roomId;
    }
}