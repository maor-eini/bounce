using HospiSaaS.Application.Dtos;
using HospiSaaS.Application.Services;
using HospiSaaS.Domain.Entities;
using HospiSaaS.Domain.Repositories;
using HospiSaaS.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace HospiSaaS.Surgery.API.Controllers;

[ApiController]
[Route("api/hospitals/{hospitalId:guid}")]
public sealed class HospitalController : ControllerBase
{
    private readonly SchedulingService _scheduler;
    private readonly IHospitalRepository _repo;

    public HospitalController(SchedulingService scheduler,
        IHospitalRepository repo)
    {
        _scheduler = scheduler;
        _repo = repo;
    }

    [HttpPost("surgeries")]
    public ActionResult<SurgeryResponseDto> RequestSurgery(
        Guid hospitalId,
        [FromBody] SurgeryRequestDto dto)
    {
        try
        {
            var result = _scheduler.Schedule(
                hospitalId,
                dto.DoctorId,
                dto.SurgeryType,
                dto.DesiredTimeUtc);

            var payload = Map(result.Request, result.WaitingPosition);

            if (result.Request.Status == RequestStatus.Scheduled)
                return CreatedAtAction(nameof(GetSurgery),
                    new { hospitalId, requestId = payload.RequestId }, payload);
            
            return Accepted(payload);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpGet("surgeries/{requestId:guid}")]
    public ActionResult<SurgeryResponseDto> GetSurgery(Guid hospitalId, Guid requestId)
    {
        var hospital = _repo.GetById(hospitalId);
        if (hospital is null) return NotFound();

        var req = hospital.Scheduled
            .Concat(hospital.WaitingList)
            .FirstOrDefault(r => r.Id == requestId);

        if (req is null) return NotFound();

        var dto = Map(req,
            req.Status == RequestStatus.Waiting
                ? hospital.GetWaitingPosition(req.Id)
                : null);

        return Ok(dto);
    }

    [HttpGet("surgeries")]
    public IActionResult GetAllSurgeries(Guid hospitalId)
    {
        var hospital = _repo.GetById(hospitalId);
        if (hospital is null) return NotFound();

        var list = hospital.Scheduled.Select(r => new
        {
            r.Id,
            r.DoctorId,
            SurgeryType = r.Type,
            StartUtc = r.DesiredTimeUtc,
            r.OperatingRoomId
        });

        return Ok(list);
    }

    [HttpGet("waiting-list")]
    public ActionResult<IEnumerable<SurgeryResponseDto>> GetWaitingList(Guid hospitalId)
    {
        var hospital = _repo.GetById(hospitalId);
        if (hospital is null) return NotFound();

        var list = hospital.WaitingList
            .Select(r => Map(r, hospital.GetWaitingPosition(r.Id)));

        return Ok(list);
    }

    [HttpPost("waiting-list/process")]
    public IActionResult ProcessWaitingList()
    {
        _scheduler.ProcessAllWaitingLists();
        return Ok(new { message = "Waiting-lists processed." });
    }

    private static SurgeryResponseDto Map(SurgeryRequest req, int? place)
    {
        return new SurgeryResponseDto(req.Id,
            req.Status.ToString(),
            req.DesiredTimeUtc,
            req.OperatingRoomId,
            place);
    }
}