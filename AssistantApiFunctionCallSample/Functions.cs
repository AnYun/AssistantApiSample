using AssistantApiFunctionCallSample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssistantApiFunctionCallSample
{
    internal class Functions
    {
        /// <summary>
        /// Cal Food Price
        /// </summary>
        /// <param name="foodInfo"></param>
        /// <returns></returns>
        public static decimal CalFoodPrice(FoodInfo foodInfo)
        {
            switch (foodInfo.Food)
            {
                case "牛排":
                    return 200 * foodInfo.Count;
                case "豬排":
                    return 100 * foodInfo.Count;
                default:
                    return 0;
            }
        }
        /// <summary>
        /// Get Current Weather
        /// </summary>
        /// <param name="foodInfo"></param>
        /// <returns></returns>
        public static string GetCurrentWeather(WeatherConfig weatherConfig)
        {
            switch (weatherConfig.Unit)
            {
                case "華式":
                    return $"{weatherConfig.Location} 目前華式 86 度";
                case "攝氏":
                default:
                    return $"{weatherConfig.Location} 目前攝氏 30 度";
            }
        }
    }
}
