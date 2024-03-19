using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AssistantApiFunctionCallSample.Models
{
    internal class WeatherConfig
    {
        [JsonPropertyName("location")]
        public string Location { get; set; }
        [JsonPropertyName("unit")]
        public string Unit { get; set; }
    }
}
