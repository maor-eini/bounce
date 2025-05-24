using HospiSaaS.Domain.Exceptions;
using HospiSaaS.Domain.ValueObjects;

namespace HospiSaaS.Domain.Entities;

/// <summary>
///     Aggregate root that owns doctors, ORs, schedule and waiting-list.
/// </summary>
public sealed class Hospital
{
    public Guid Id { get; private init; }
    public string Name { get; private init; } = string.Empty;

    private readonly List<Doctor> _doctors = new();
    private readonly List<OperatingRoom> _rooms = new();
    private readonly List<SurgeryRequest> _waitingList = new();
    private readonly List<SurgeryRequest> _scheduled = new();

    public IReadOnlyCollection<SurgeryRequest> WaitingList => _waitingList.AsReadOnly();
    public IReadOnlyCollection<SurgeryRequest> Scheduled => _scheduled.AsReadOnly();

    private readonly object _waitingLock = new();

    private Hospital()
    {
    }

    public static Hospital Create(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hospital name is required.", nameof(name));

        return new Hospital { Id = id, Name = name };
    }

    public void AddDoctor(Doctor d)
    {
        if (d.HospitalId != Id) throw new InvalidOperationException("Doctor belongs to another hospital.");
        _doctors.Add(d);
    }

    public void AddOperatingRoom(OperatingRoom r)
    {
        if (r.HospitalId != Id) throw new InvalidOperationException("OR belongs to another hospital.");
        _rooms.Add(r);
    }

    public SurgeryRequest RequestSurgery(Guid doctorId, SurgeryType type, DateTime startUtc)
    {
        var doctor = _doctors.FirstOrDefault(d => d.Id == doctorId)
                     ?? throw new DomainException("Doctor not found in hospital.");

        if (doctor.Specialty != type)
            throw new DomainException("Doctor's specialty does not match requested surgery type.");

        ValidateTime(startUtc);

        foreach (var room in _rooms.Where(r => r.CanPerform(type)))
            if (room.TryBook(type, startUtc))
            {
                var sched = SurgeryRequest.Scheduled(doctor.Id, type, startUtc, room.Id);
                _scheduled.Add(sched);
                return sched;
            }

        var waiting = SurgeryRequest.Waiting(doctor.Id, type, startUtc);
        lock (_waitingLock)
        {
            _waitingList.Add(waiting);
        }

        return waiting;
    }

    public void ProcessWaitingList()
    {
        List<SurgeryRequest> snapshot;
        lock (_waitingLock)
        {
            snapshot = _waitingList.ToList();
        }

        foreach (var req in snapshot)
        foreach (var room in _rooms.Where(r => r.CanPerform(req.Type)))
            if (room.TryBook(req.Type, req.DesiredTimeUtc))
            {
                lock (_waitingLock)
                {
                    _waitingList.Remove(req);
                }

                req.MarkScheduled(room.Id);
                _scheduled.Add(req);

                break;
            }
    }

    public int GetWaitingPosition(Guid requestId)
    {
        lock (_waitingLock)
        {
            var idx = _waitingList.FindIndex(r => r.Id == requestId);
            return idx < 0 ? -1 : idx + 1;
        }
    }

    private static void ValidateTime(DateTime utc)
    {
        var local = utc.ToLocalTime();

        if (local < DateTime.Now || local > DateTime.Now.AddDays(7))
            throw new DomainException("Surgery must be within the next 7 days.");

        var hour = local.TimeOfDay;
        if (hour < TimeSpan.FromHours(10) || hour >= TimeSpan.FromHours(18))
            throw new DomainException("Surgery must start between 10:00 and 18:00.");
    }
}