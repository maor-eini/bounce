namespace HospiSaaS.Application.Interfaces
{
    public interface ISchedulerService
    {
        // Process an event (expects SurgeryRequestDto for now)
        void ScheduleSurgery(object eventDto);
    }
}