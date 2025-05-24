using HospiSaaS.Domain.Exceptions;
using HospiSaaS.Domain.ValueObjects;

namespace HospiSaaS.Domain.Entities;

public class Hospital 
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    private List<Doctor> _doctors = [];
    private List<OperatingRoom> _operatingRooms = [];
    private List<SurgeryRequest> _pendingRequests = [];
    private List<SurgeryRequest> _scheduledSurgeries = [];
    
    public IReadOnlyCollection<SurgeryRequest> PendingRequests => _pendingRequests.AsReadOnly();
    public IReadOnlyCollection<SurgeryRequest> ScheduledSurgeries => _scheduledSurgeries.AsReadOnly();
    public SurgeryRequest ScheduleSurgery(Guid doctorId, SurgeryType type, DateTime dateTime) {

        var doctor = _doctors.FirstOrDefault(d => d.Id == doctorId);
        if (doctor == null) throw new DomainException($"Doctor {doctorId} not found in hospital {Name}");
        if (doctor.Specialty != type) {
            throw new DomainException($"Doctor's specialty {doctor.Specialty} does not match requested surgery type {type}");
        }
        
        if (dateTime < DateTime.Now || dateTime > DateTime.Now.AddDays(7)) {
            throw new DomainException("Surgery time must be within the next 7 days");
        }
        var hour = dateTime.TimeOfDay;
        if (hour < TimeSpan.FromHours(10) || hour >= TimeSpan.FromHours(18)) {
            throw new DomainException("Surgery time must be between 10:00 and 18:00");
        }

        var assignedRoom = _operatingRooms
            .Where(r => r.CanPerform(type))
            .FirstOrDefault(r => r.IsFreeAt(dateTime));

        if (assignedRoom != null) {
            assignedRoom.Book(dateTime);
            var request = new SurgeryRequest(Guid.NewGuid(), doctorId, type, dateTime, SurgeryStatus.Scheduled, assignedRoom.Id);
            _scheduledSurgeries.Add(request);
            return request;
        } else {
            var request = new SurgeryRequest(Guid.NewGuid(), doctorId, type, dateTime, SurgeryStatus.Pending);
            _pendingRequests.Add(request);
            return request;
        }
    }


    public void ProcessPendingRequests() 
    {
        for (var i = _pendingRequests.Count - 1; i >= 0; i--) {
            var req = _pendingRequests[i];
            var availableRoom = _operatingRooms
                .Where(r => r.CanPerform(req.Type))
                .FirstOrDefault(r => r.IsFreeAt(req.ScheduledTime));
            
            if (availableRoom != null) {
                availableRoom.Book(req.ScheduledTime);
                req.MarkScheduled(availableRoom.Id);
                _scheduledSurgeries.Add(req);
                _pendingRequests.RemoveAt(i);
            }
        }
    }
    
    public static Hospital Create(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hospital name is required.");

        return new Hospital
        {
            Id = id,
            Name = name
        };
    }

    public void AddDoctor(Doctor doctor)
    {
        if (doctor.HospitalId != Id)
            throw new InvalidOperationException("Doctor does not belong to this hospital.");
        _doctors.Add(doctor);
    }

    public void AddOperatingRoom(OperatingRoom or)
    {
        if (or.HospitalId != Id)
            throw new InvalidOperationException("Operating room does not belong to this hospital.");
        _operatingRooms.Add(or);
    }
}