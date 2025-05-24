using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using HospiSaaS.Domain.Repositories;
using HospiSaaS.Infrastructure.Data;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace HospiSaaS.Tests
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var provider = services.BuildServiceProvider();
                        using var scope = provider.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<IHospitalRepository>();
                        SampleDataSeeder.Seed(repo);
                    });
                })
                .CreateClient();
        }

        [Fact]
        public async Task RequestSurgery_ValidRequest_SchedulesSuccessfully() 
        {
            var hospitalId = SeedConstants.HospitalId;
            var doctorId = SeedConstants.DoctorMosheId;

            var request = new {
                doctorId = doctorId,
                surgeryType = 0,
                desiredTime = DateTime.UtcNow.Date.AddHours(10).AddDays(1)  // 10:00 tomorrow
            };
            
            var response = await _client.PostAsJsonAsync($"/api/hospitals/{hospitalId}/surgeries", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var responseData = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            Assert.NotNull(responseData);
            Assert.Equal("Scheduled", responseData["status"].ToString());
            Assert.NotNull(responseData["operatingRoomId"].ToString());
        }
        
        [Fact]
        public async Task RequestSurgery_WhenORIsFreeAtDifferentTime_SchedulesInsteadOfQueues()
        {
            // Given: Hospital with at least 2 Heart surgery-capable ORs
            var hospitalId = SeedConstants.HospitalId;
            var heartSurgeonId = SeedConstants.DoctorMosheId;
            var secondHeartSurgeonId = SeedConstants.DoctorMayaId;
            var thirdHeartSurgeonId = SeedConstants.DoctorLilyId;

            var timeSlot = DateTime.UtcNow.Date.AddDays(1).AddHours(11);       // 11:00
            var thirdTimeSlot = timeSlot.AddHours(1);                          // 12:00

            // Schedule 11:00 in two ORs
            await _client.PostAsJsonAsync($"/api/hospitals/{hospitalId}/surgeries", new
            {
                doctorId = heartSurgeonId,
                surgeryType = 0, // Heart
                desiredTime = timeSlot
            });

            await _client.PostAsJsonAsync($"/api/hospitals/{hospitalId}/surgeries", new
            {
                doctorId = secondHeartSurgeonId,
                surgeryType = 0, // Heart
                desiredTime = timeSlot
            });

            // Third request at 12:00 → expected to succeed immediately
            var response = await _client.PostAsJsonAsync($"/api/hospitals/{hospitalId}/surgeries", new
            {
                doctorId = thirdHeartSurgeonId,
                surgeryType = 0, // Heart
                desiredTime = thirdTimeSlot
            });

            // ✅ Should be scheduled immediately, not queued
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var data = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            Assert.NotNull(data);
            Assert.Equal("Scheduled", data["status"].ToString());
            Assert.False(string.IsNullOrEmpty(data["operatingRoomId"]?.ToString()));
        }

        
        
    }
}