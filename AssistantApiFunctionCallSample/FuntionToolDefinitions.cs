using Azure.AI.OpenAI.Assistants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AssistantApiFunctionCallSample
{
    internal class FunctionToolDefinitions
    {
        /// <summary>
        /// Get FuntionDefinitions
        /// </summary>
        /// <returns></returns>
        public static FunctionToolDefinition GetFoodPriceFuntionToolDefinition()
        {
            var calFoodPriceFuntionToolDefinition = new FunctionToolDefinition(
                name: "CalFoodPrice",
                description: "計算客戶餐點價錢",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        Type = "object",
                        Properties = new
                        {
                            Count = new
                            {
                                Type = "integer",
                                Description = "客戶點的餐點數量，比如說一份"
                            },
                            Food = new
                            {
                                Type = "string",
                                Enum = new[] { "牛排", "豬排" },
                                Description = "客戶點的餐點，比如說牛排或是豬排。"
                            }
                        },
                        Required = new[] { "count", "food" },
                    }, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                )
            ;

            return calFoodPriceFuntionToolDefinition;
        }

        public static FunctionToolDefinition GetWeatherFuntionToolDefinition()
        {
            var getWeatherFuntionToolDefinition = new FunctionToolDefinition(
                name: "GetCurrentWeather",
                description: "取得指定地點的天氣資訊",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        Type = "object",
                        Properties = new
                        {
                            Location = new
                            {
                                Type = "string",
                                Description = "城市或鄉鎮地點",
                            },
                            Unit = new
                            {
                                Type = "string",
                                Enum = new[] { "攝氏", "華式" },
                            }
                        },
                        Required = new[] { "location" },
                    }, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                );

            return getWeatherFuntionToolDefinition;
        }
    }
}
