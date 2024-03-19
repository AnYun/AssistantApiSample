using Azure.AI.OpenAI.Assistants;
using Azure;

namespace AssistantApiSample
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
                Instructions = "你是 Azure 專家，會回覆關於 Azure 的問題。"
            });
            Assistant assistant = assistantResponse.Value;

            // 2. 建立聊天串
            Response<AssistantThread> threadResponse = await client.CreateThreadAsync();
            AssistantThread thread = threadResponse.Value;

            Response<ThreadMessage> messageResponse = await client.CreateMessageAsync(thread.Id, MessageRole.User, "如何建立一台 VM ?");
            ThreadMessage message = messageResponse.Value;

            // 3. 運行助理回覆問題
            Response<ThreadRun> runResponse = await client.CreateRunAsync(thread.Id, new CreateRunOptions(assistant.Id));
            ThreadRun run = runResponse.Value;

            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                runResponse = await client.GetRunAsync(thread.Id, runResponse.Value.Id);
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
    }
}
