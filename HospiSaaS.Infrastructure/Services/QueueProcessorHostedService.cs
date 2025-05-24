using HospiSaaS.Application.Interfaces;
using HospiSaaS.Domain.Repositories;
using Microsoft.Extensions.Hosting;

namespace HospiSaaS.Infrastructure.Services;

public class QueueProcessorHostedService : BackgroundService 
{
    private readonly IHospitalRepository _hospitalRepo;
    private readonly INotifier _notifier;
    private readonly TimeSpan _executionInterval = TimeSpan.FromMinutes(1);

    public QueueProcessorHostedService(IHospitalRepository hospitalRepo, INotifier notifier) {
        _hospitalRepo = hospitalRepo;
        _notifier = notifier;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
    {
        _notifier.Notify("Queue Processor Service running.");
        while (!stoppingToken.IsCancellationRequested) 
        {
            try {
                ProcessAllHospitalQueues();
            }
            catch (Exception ex) {
                _notifier.Notify($"Error processing queues. details: {ex.Message}");
            }
            await Task.Delay(_executionInterval, stoppingToken);
        }
    }

    private void ProcessAllHospitalQueues() {
        foreach (var hospital in _hospitalRepo.GetAll()) 
        {
            if (hospital.PendingRequests.Any()) {
                _notifier.Notify($"Processing queue for hospital {hospital.Name}");
                hospital.ProcessPendingRequests();
                _hospitalRepo.Update(hospital);
            }
        }
    }
}
