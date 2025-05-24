using System.Text.Json.Serialization;

namespace HospiSaaS.Surgery.API.Models
{
    public class SurgeryRequest
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("repo_id")]
        public string RepoId { get; set; }
        
        [JsonPropertyName("team_name")]
        public string TeamName { get; set; }
        
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}