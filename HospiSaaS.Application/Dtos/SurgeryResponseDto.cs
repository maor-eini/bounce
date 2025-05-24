namespace HospiSaaS.Application.Dtos;

public record SurgeryResponseDto(
    Guid   RequestId,
    string Status,
    DateTime DesiredTimeUtc,
    Guid?  OperatingRoomId,
    int?   WaitingPosition);