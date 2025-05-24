using HospiSaaS.Application.Interfaces;
using HospiSaaS.Domain.Repositories;
using Microsoft.Extensions.Hosting;

namespace HospiSaaS.Infrastructure.Services;

public sealed class WaitingListProcessorHostedService : BackgroundService
{
    private readonly IHospitalRepository _hospitalRepo;
    private readonly INotifier _notifier;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public WaitingListProcessorHostedService(
        IHospitalRepository hospitalRepo,
        INotifier notifier)
    {
        _hospitalRepo = hospitalRepo;
        _notifier = notifier;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _notifier.Notify("Waiting-list processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                ProcessAllHospitals();
            }
            catch (Exception ex)
            {
                _notifier.Notify($"Waiting-list processor error: {ex.Message}");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private void ProcessAllHospitals()
    {
        foreach (var hospital in _hospitalRepo.GetAll())
        {
            if (!hospital.WaitingList.Any()) continue;

            _notifier.Notify($"Promoting waiting-list for {hospital.Name}");

            hospital.ProcessWaitingList();
            _hospitalRepo.Update(hospital);
        }
    }
}