namespace HospiSaaS.Domain.ValueObjects;

/// <summary>Immutable time-range wrapper used for overlap checks.</summary>
public record TimeSlot(DateTime StartUtc, int DurationHours)
{
    public DateTime EndUtc => StartUtc.AddHours(DurationHours);

    public bool Overlaps(TimeSlot other) =>
        StartUtc < other.EndUtc && EndUtc > other.StartUtc;
}
