using HospiSaaS.Domain.Entities;
using HospiSaaS.Domain.Repositories;
using HospiSaaS.Domain.ValueObjects;

namespace HospiSaaS.Application.Services;

public sealed class SchedulingService
{
    private readonly IHospitalRepository _repo;

    public SchedulingService(IHospitalRepository repo)
    {
        _repo = repo;
    }
    
    public SchedulingResult Schedule(
        Guid hospitalId,
        Guid doctorId,
        SurgeryType surgeryType,
        DateTime desiredStartUtc)
    {
        var hospital = _repo.GetById(hospitalId)
                       ?? throw new KeyNotFoundException("Hospital not found.");

        var req = hospital.RequestSurgery(doctorId, surgeryType, desiredStartUtc);

        int? place = req.Status == RequestStatus.Waiting
            ? hospital.GetWaitingPosition(req.Id)
            : null;

        return new SchedulingResult(req, place);
    }
    
    public void ProcessAllWaitingLists()
    {
        foreach (var hospital in _repo.GetAll())
            hospital.ProcessWaitingList();
    }

    public readonly record struct SchedulingResult(
        SurgeryRequest Request,
        int? WaitingPosition);
}