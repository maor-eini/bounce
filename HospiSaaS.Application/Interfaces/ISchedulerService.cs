namespace HospiSaaS.Application.Interfaces
{
    public interface ISchedulerService
    {
        void ScheduleSurgery(object eventDto);
    }
}