using Azure;
using Azure.AI.OpenAI;
using OpenAI.Assistants;

namespace AssistantApiSample
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
            AssistantClient client = new AzureOpenAIClient(new Uri(azureResourceUrl), new AzureKeyCredential(azureApiKey)).GetAssistantClient();
            
            // 1. 建立助理
            Assistant assistant = await client.CreateAssistantAsync(
                model: deploymentName,
                new AssistantCreationOptions()
                {
                    Name = "DEMO 助理",
                    Instructions = "你是 Azure 專家，會回覆關於 Azure 的問題。"
                });

            // 2. 建立聊天串
            AssistantThread thread = await client.CreateThreadAsync();

            var messageResponse = await client.CreateMessageAsync(thread,
                [
                    "如何建立一台 VM?"
                ]
            );

            // 3. 運行助理回覆問題
            var runResponse = await client.CreateRunAsync(thread, assistant);
            ThreadRun run = runResponse.Value;

            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                runResponse = await client.GetRunAsync(thread.Id, runResponse.Value.Id);
            }
            while (runResponse.Value.Status == RunStatus.Queued || runResponse.Value.Status == RunStatus.InProgress);

            // 4. 顯示出助理運行完後的聊天串
            var afterRunMessagesResponse = client.GetMessagesAsync(thread);

            await foreach (ThreadMessage threadMessage in afterRunMessagesResponse)
            {
                Console.WriteLine($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
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

            var deleteThread = await client.DeleteThreadAsync(thread.Id);
            var deleteAssistant = await client.DeleteAssistantAsync(assistant.Id);

            Console.WriteLine($"Delete Thread {thread.Id} {deleteThread.Value}");
            Console.WriteLine($"Delete Assistant {assistant.Id} {deleteAssistant.Value}");
        }
    }
}
