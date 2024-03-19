using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AssistantApiFunctionCallSample.Models
{
    internal class FoodInfo
    {
        [JsonPropertyName("food")]
        public string Food { get; set; }
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
