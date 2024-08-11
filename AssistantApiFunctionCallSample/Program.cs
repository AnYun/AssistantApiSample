using Azure;
using System.Text.Json;
using AssistantApiFunctionCallSample.Models;
using Azure.AI.OpenAI;
using OpenAI.Assistants;

namespace AssistantApiFunctionCallSample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var azureResourceUrl = "https://{your account}.openai.azure.com/";
            var azureApiKey = "{Your Api Key}";
            var deploymentName = "{Your Model Name}";
            // Assistants is a beta API and subject to change; acknowledge its experimental status by suppressing the matching warning.
#pragma warning disable OPENAI001
            AssistantClient assistantClient = new AzureOpenAIClient(new Uri(azureResourceUrl), new AzureKeyCredential(azureApiKey)).GetAssistantClient();

            // 1. 建立助理
            Assistant assistant = await assistantClient.CreateAssistantAsync(
                model: deploymentName,
                new AssistantCreationOptions()
                {
                    Name = "DEMO 助理",
                    Tools = {
                        FunctionToolDefinitions.GetFoodPriceFuntionToolDefinition(),
                        FunctionToolDefinitions.GetWeatherFuntionToolDefinition()
                    }
                });

            // 2. 建立聊天串
            AssistantThread thread = await assistantClient.CreateThreadAsync(new ThreadCreationOptions()
            {
                InitialMessages = { new ThreadInitializationMessage(
                [
                    "我想要點兩份牛排"
			        //"請問台北市的天氣?"
			        //"請問 PS5 多少錢?"
		        ]) }
            });

            // 3. 運行助理回覆問題
            ThreadRun threadRun = await assistantClient.CreateRunAsync(thread, assistant);

            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                threadRun = await assistantClient.GetRunAsync(thread.Id, threadRun.Id);

                if (threadRun.Status == RunStatus.RequiresAction)
                {
                    List<ToolOutput> toolOutputs = new();
                    foreach (RequiredAction action in threadRun.RequiredActions)
                    {
                        toolOutputs.Add(GetResolvedToolOutput(action));
                    }
                }
            }
            while (threadRun.Status == RunStatus.Queued || threadRun.Status == RunStatus.InProgress);

            // 4. 顯示出助理運行完後的聊天串
            var threadMessages = assistantClient.GetMessagesAsync(thread);

            await foreach (ThreadMessage threadMessage in threadMessages)
            {
                Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
                foreach (MessageContent contentItem in threadMessage.Content)
                {
                    if (!string.IsNullOrEmpty(contentItem.Text))
                    {
                        Console.Write(contentItem.Text);
                    }
                    else if (!string.IsNullOrEmpty(contentItem.ImageFileId))
                    {
                        Console.Write($"<image from ID: {contentItem.ImageFileId}");
                    }
                    Console.WriteLine();
                }
            }

            var deleteThread = await assistantClient.DeleteThreadAsync(thread.Id);
            var deleteAssistant = await assistantClient.DeleteAssistantAsync(assistant.Id);

            Console.WriteLine($"Delete Thread {thread.Id} {deleteThread.Value}");
            Console.WriteLine($"Delete Assistant {assistant.Id} {deleteAssistant.Value}");
        }
        /// <summary>
        /// ToolOutput
        /// </summary>
        /// <param name="toolCall"></param>
        /// <returns></returns>
        static ToolOutput GetResolvedToolOutput(RequiredAction requiredAction)
        {
            switch (requiredAction.FunctionName)
            {
                case "CalFoodPrice":
                    var foodInfo = JsonSerializer.Deserialize<FoodInfo>(requiredAction.FunctionArguments);
                    var foodPrice = Functions.CalFoodPrice(foodInfo);
                    return new ToolOutput(requiredAction.ToolCallId, $"您點了 {foodInfo.Count} 份 {foodInfo.Food} 總共 {foodPrice} 元");
                case "GetCurrentWeather":
                    var weatherConfig = JsonSerializer.Deserialize<WeatherConfig>(requiredAction.FunctionArguments);
                    return new ToolOutput(requiredAction.ToolCallId, Functions.GetCurrentWeather(weatherConfig));
                default:
                    break;
            }

            return null;
        }
    }
}
