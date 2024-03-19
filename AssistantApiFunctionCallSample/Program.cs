using Azure.AI.OpenAI.Assistants;
using Azure;
using System.Text.Json;
using AssistantApiFunctionCallSample.Models;

namespace AssistantApiFunctionCallSample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var azureResourceUrl = "{Your Azure Resource Url}";
            var azureApiKey = "{Your Azure Api Key}";
            AssistantsClient client = new AssistantsClient(new Uri(azureResourceUrl), new AzureKeyCredential(azureApiKey));

            // 1. 建立助理
            Response<Assistant> assistantResponse = await client.CreateAssistantAsync(
            new AssistantCreationOptions("{Your Model Name}")
            {
                Name = "DEMO 助理",
                Tools = {
                    FunctionToolDefinitions.GetFoodPriceFuntionToolDefinition(),
                    FunctionToolDefinitions.GetWeatherFuntionToolDefinition()
                }
            });
            Assistant assistant = assistantResponse.Value;

            // 2. 建立聊天串
            Response<AssistantThread> threadResponse = await client.CreateThreadAsync();
            AssistantThread thread = threadResponse.Value;

            Response<ThreadMessage> messageResponse = await client.CreateMessageAsync(thread.Id, MessageRole.User,
                //"我想要點兩份牛排"
                //"請問台北市的天氣?"
                "請問 PS5 多少錢?"
            );
            ThreadMessage message = messageResponse.Value;

            // 3. 運行助理回覆問題
            Response<ThreadRun> runResponse = await client.CreateRunAsync(thread.Id, new CreateRunOptions(assistant.Id));
            ThreadRun run = runResponse.Value;

            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                runResponse = await client.GetRunAsync(thread.Id, runResponse.Value.Id);

                if (runResponse.Value.Status == RunStatus.RequiresAction
                    && runResponse.Value.RequiredAction is SubmitToolOutputsAction submitToolOutputsAction)
                {
                    List<ToolOutput> toolOutputs = new();
                    foreach (RequiredToolCall toolCall in submitToolOutputsAction.ToolCalls)
                    {
                        toolOutputs.Add(GetResolvedToolOutput(toolCall));
                    }
                    runResponse = await client.SubmitToolOutputsToRunAsync(runResponse.Value, toolOutputs);
                }
            }
            while (runResponse.Value.Status == RunStatus.Queued || runResponse.Value.Status == RunStatus.InProgress);

            // 4. 顯示出助理運行完後的聊天串
            Response<PageableList<ThreadMessage>> afterRunMessagesResponse = await client.GetMessagesAsync(thread.Id);
            IReadOnlyList<ThreadMessage> messages = afterRunMessagesResponse.Value.Data;

            foreach (ThreadMessage threadMessage in messages.OrderBy(x => x.CreatedAt))
            {
                Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
                foreach (MessageContent contentItem in threadMessage.ContentItems)
                {
                    if (contentItem is MessageTextContent textItem)
                    {
                        Console.Write(textItem.Text);
                    }
                    else if (contentItem is MessageImageFileContent imageFileItem)
                    {
                        Console.Write($"<image from ID: {imageFileItem.FileId}");
                    }
                    Console.WriteLine();
                }
            }

            var deleteThread = await client.DeleteThreadAsync(thread.Id);
            var deleteAssistant = await client.DeleteAssistantAsync(assistant.Id);

            Console.WriteLine($"Delete Thread {thread.Id} {deleteThread.Value}");
            Console.WriteLine($"Delete Assistant {assistant.Id} {deleteAssistant.Value}");
        }
        /// <summary>
        /// ToolOutput
        /// </summary>
        /// <param name="toolCall"></param>
        /// <returns></returns>
        static ToolOutput GetResolvedToolOutput(RequiredToolCall toolCall)
        {
            if (toolCall is RequiredFunctionToolCall functionToolCall)
            {
                switch (functionToolCall.Name)
                {
                    case "CalFoodPrice":
                        var foodInfo = JsonSerializer.Deserialize<FoodInfo>(functionToolCall.Arguments);
                        var foodPrice = Functions.CalFoodPrice(foodInfo);
                        return new ToolOutput(toolCall, $"您點了 {foodInfo.Count} 份 {foodInfo.Food} 總共 {foodPrice} 元");
                    case "GetCurrentWeather":
                        var weatherConfig = JsonSerializer.Deserialize<WeatherConfig>(functionToolCall.Arguments);
                        return new ToolOutput(toolCall, Functions.GetCurrentWeather(weatherConfig));
                    default:
                        break;
                }
            }
            return null;
        }
    }
}
