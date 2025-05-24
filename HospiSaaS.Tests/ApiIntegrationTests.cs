using System.Net;
using System.Net.Http.Json;
using HospiSaaS.Domain.Repositories;
using HospiSaaS.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace HospiSaaS.Tests;

public class ApiIntegrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _client = factory
            .WithWebHostBuilder(b =>
            {
                b.ConfigureServices(services =>
                {
                    using var scope = services.BuildServiceProvider().CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IHospitalRepository>();
                    repo.Clear();
                    SampleDataSeeder.Seed(repo);
                });
            })
            .CreateClient();
    }

    [Fact]
    public async Task RequestSurgery_ValidRequest_SchedulesSuccessfully()
    {
        var hospitalId = SeedConstants.HospitalId;
        var body = new
        {
            doctorId = SeedConstants.DoctorMosheId,
            surgeryType = "Heart",
            desiredTimeUtc = DateTime.UtcNow.Date
                .AddDays(1)
                .AddHours(12)
        };

        var resp = await _client.PostAsJsonAsync(
            $"/api/hospitals/{hospitalId}/surgeries", body);

        if (resp.StatusCode != HttpStatusCode.Created)
            _testOutputHelper.WriteLine("Server response:" + await resp.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

        var json = await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("Scheduled", json["status"]?.ToString());
        Assert.False(string.IsNullOrEmpty(json["operatingRoomId"]?.ToString()));
    }

    [Fact]
    public async Task RequestSurgery_WhenORFreeLater_Schedules()
    {
        // Heart surgeon at 20:00 -> 400 (outside working hours)
        
        var hid = SeedConstants.HospitalId;
        var doc1 = SeedConstants.DoctorMosheId;
        var doc2 = SeedConstants.DoctorMayaId;
        var doc3 = SeedConstants.DoctorLilyId;

        var elevenUtc = DateTime.UtcNow.Date.AddDays(1).AddHours(11);
        var noonUtc = elevenUtc.AddHours(1);

        await _client.PostAsJsonAsync($"/api/hospitals/{hid}/surgeries", new
        {
            doctorId = doc1, surgeryType = "Heart", desiredTimeUtc = elevenUtc
        });
        await _client.PostAsJsonAsync($"/api/hospitals/{hid}/surgeries", new
        {
            doctorId = doc2, surgeryType = "Heart", desiredTimeUtc = elevenUtc
        });

        var resp = await _client.PostAsJsonAsync($"/api/hospitals/{hid}/surgeries", new
        {
            doctorId = doc3, surgeryType = "Heart", desiredTimeUtc = noonUtc
        });

        if (resp.StatusCode != HttpStatusCode.Created)
            _testOutputHelper.WriteLine($"Server response: {await resp.Content.ReadAsStringAsync()}");

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

        var json = await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("Scheduled", json["status"]?.ToString());
        Assert.False(string.IsNullOrEmpty(json["operatingRoomId"]?.ToString()));
    }
    
    [Fact]
    public async Task RequestSurgery_AfterHours_ReturnsBadRequest()
    {
        var body = new
        {
            doctorId = SeedConstants.DoctorMosheId,
            surgeryType = "Heart",
            desiredTimeUtc = DateTime.UtcNow.Date.AddDays(1).AddHours(22) // 22:00 local
        };

        var resp = await _client.PostAsJsonAsync(
            $"/api/hospitals/{SeedConstants.HospitalId}/surgeries", body);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        Assert.Contains("10:00 and 18:00",
            await resp.Content.ReadAsStringAsync());
    }
    

    
    [Fact]
    public async Task BrainSurgery_CannotOverlapWithinDuration()
    {
        // Brain surgeon in OR with CT -> 2-hour overlap test
        // Book at 10:00, second request at 11:00 should be rejected.
        
        var hospitalId = SeedConstants.HospitalId;
        var brainDoc = SeedConstants.DoctorYitzhakId;
        var tenUtc = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var elevenUtc = tenUtc.AddHours(1); // overlaps 2-h slot

        // First booking succeeds
        var ok = await _client.PostAsJsonAsync($"/api/hospitals/{hospitalId}/surgeries", new
        {
            doctorId = brainDoc, surgeryType = "Brain", desiredTimeUtc = tenUtc
        });
        Assert.Equal(HttpStatusCode.Created, ok.StatusCode);

        // Second overlaps same OR -> should be Waiting (202)
        var conflicted = await _client.PostAsJsonAsync($"/api/hospitals/{hospitalId}/surgeries", new
        {
            doctorId = brainDoc, surgeryType = "Brain", desiredTimeUtc = elevenUtc
        });
        Assert.Equal(HttpStatusCode.Accepted, conflicted.StatusCode);
    }
    
    [Fact]
    public async Task WrongSpecialty_ReturnsBadRequest()
    {
        var resp = await _client.PostAsJsonAsync(
            $"/api/hospitals/{SeedConstants.HospitalId}/surgeries", new
            {
                doctorId = SeedConstants.DoctorYitzhakId, // Brain doc
                surgeryType = "Heart",
                desiredTimeUtc = DateTime.UtcNow.Date.AddDays(1).AddHours(12)
            });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        Assert.Contains("specialty", await resp.Content.ReadAsStringAsync());
    }
    
    [Fact]
    public async Task WaitingList_ManualProcess_PromotesRequest()
    {
        var hid = SeedConstants.HospitalId;
        var docId = SeedConstants.DoctorMosheId;
        var slotUtc = DateTime.UtcNow.Date.AddDays(1).AddHours(17); // near closing

        // Book both heart ORs at 17:00 so third request waits
        for (var i = 0; i < 2; i++)
            await _client.PostAsJsonAsync($"/api/hospitals/{hid}/surgeries", new
            {
                doctorId = i == 0 ? docId : SeedConstants.DoctorMayaId,
                surgeryType = "Heart",
                desiredTimeUtc = slotUtc
            });

        // Third request queued
        var queued = await _client.PostAsJsonAsync($"/api/hospitals/{hid}/surgeries", new
        {
            doctorId = SeedConstants.DoctorLilyId,
            surgeryType = "Heart",
            desiredTimeUtc = slotUtc
        });
        Assert.Equal(HttpStatusCode.Accepted, queued.StatusCode);
        var qData = await queued.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var requestId = qData["requestId"];

        // Free an OR by advancing time 3h (simulate) or just processing for next day
        await _client.PostAsync($"/api/hospitals/{hid}/waiting-list/process", null);

        // Check status -> should now be Scheduled
        var status = await _client.GetAsync(
            $"/api/hospitals/{hid}/surgeries/{requestId}");
        var sData = await status.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.Equal("Scheduled", sData["status"]?.ToString());
    }
    
    [Fact]
    public async Task RequestBeyondOneWeek_ReturnsBadRequest()
    {
        var resp = await _client.PostAsJsonAsync(
            $"/api/hospitals/{SeedConstants.HospitalId}/surgeries", new
            {
                doctorId = SeedConstants.DoctorMosheId,
                surgeryType = "Heart",
                desiredTimeUtc = DateTime.UtcNow.AddDays(8) // > 7 days
            });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        Assert.Contains("7 days", await resp.Content.ReadAsStringAsync());
    }
}