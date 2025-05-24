using HospiSaaS.Domain.Entities;

namespace HospiSaaS.Domain.ValueObjects;

public enum SurgeryType
{
    Heart,
    Brain
}

public static class SurgeryTypeExtensions
{
    public static int GetDurationHours(this SurgeryType type, OperatingRoom room)
    {
        return type switch
        {
            SurgeryType.Heart => 3,
            SurgeryType.Brain => room.SetOfEquipment.Contains(Equipment.CT) ? 2 : 3,
            _ => 2
        };
    }
}
