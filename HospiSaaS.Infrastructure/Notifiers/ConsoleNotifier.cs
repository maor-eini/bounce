using System;
using HospiSaaS.Application.Interfaces;

namespace HospiSaaS.Infrastructure.Notifiers
{
    public class ConsoleNotifier : INotifier
    {
        public void Notify(string message)
        {
            Console.WriteLine($"[HospiSaaS] {message}");
        }
    }
}