using Microsoft.AspNetCore.Mvc;
using HospiSaaS.Application.Dtos;
using HospiSaaS.Application.Services;
using HospiSaaS.Domain.Entities;
using HospiSaaS.Domain.Exceptions;
using HospiSaaS.Domain.Repositories;

namespace HospiSaaS.Surgery.API.Controllers
{

[ApiController]
[Route("api/hospitals/{hospitalId}")]
public class HospitalController : ControllerBase 
{
    private readonly SchedulingService _schedulingService;
    private readonly IHospitalRepository _hospitalRepository;

    public HospitalController(SchedulingService schedulingService, IHospitalRepository hospitalRepository)
    {
        _schedulingService = schedulingService;
        _hospitalRepository = hospitalRepository;
    }

    [HttpPost("surgeries")]
    public IActionResult RequestSurgery(Guid hospitalId, [FromBody] SurgeryRequestDto requestDto) 
    {
        try {
            var result = _schedulingService.RequestSurgery(
                hospitalId,
                requestDto.DoctorId,
                requestDto.SurgeryType,
                requestDto.DesiredTime
            );
            
            // Prepare response DTO (could map domain to DTO if needed)
            var response = new {
                RequestId = result.Id,
                DoctorId = result.DoctorId,
                SurgeryType = result.Type,
                ScheduledTime = result.ScheduledTime,
                Status = result.Status.ToString(),
                OperatingRoomId = result.OperatingRoomId
            };
            
            if (result.Status == SurgeryStatus.Scheduled) {

                return CreatedAtAction(
                    nameof(GetSurgery), 
                    new { hospitalId = hospitalId, requestId = result.Id }, 
                    response
                );
            }
            
            return StatusCode(StatusCodes.Status202Accepted, response);
        }
        catch (DomainException ex) {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex) { // no hospital or doctor found
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("surgeries/{requestId}")]
    public IActionResult GetSurgery(Guid hospitalId, Guid requestId) {
        var hospital = _hospitalRepository.GetById(hospitalId);
        if (hospital == null) return NotFound();
        
        var req = hospital.ScheduledSurgeries.FirstOrDefault(x => x.Id == requestId) 
                  ?? hospital.PendingRequests.FirstOrDefault(x => x.Id == requestId);
        
        if (req == null) return NotFound();

        return Ok(new {
            RequestId = req.Id,
            DoctorId = req.DoctorId,
            SurgeryType = req.Type,
            ScheduledTime = req.ScheduledTime,
            Status = req.Status.ToString(),
            OperatingRoomId = req.OperatingRoomId
        });
    }

    [HttpGet("surgeries")]
    public IActionResult GetAllSurgeries(Guid hospitalId) {
        var hospital = _hospitalRepository.GetById(hospitalId);
        if (hospital == null) return NotFound();
        
        var scheduled = hospital.ScheduledSurgeries.Select(req => new {
            RequestId = req.Id,
            DoctorId = req.DoctorId,
            SurgeryType = req.Type,
            Time = req.ScheduledTime,
            OperatingRoomId = req.OperatingRoomId
        });
        
        return Ok(scheduled);
    }

    [HttpGet("queue")]
    public IActionResult GetPendingRequests(Guid hospitalId) {
        var hospital = _hospitalRepository.GetById(hospitalId);
        if (hospital == null) return NotFound();
        
        var pending = hospital.PendingRequests.Select(req => new {
            RequestId = req.Id,
            DoctorId = req.DoctorId,
            SurgeryType = req.Type,
            DesiredTime = req.ScheduledTime
        });
        
        return Ok(pending);
    }

    [HttpPost("queue/process")]
    public IActionResult ProcessQueue(Guid hospitalId) {
        _schedulingService.ProcessHospitalQueue(hospitalId);
        return Ok();
    }
}

}