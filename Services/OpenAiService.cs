using CarInsuranceTgBot.Abstractions;
using System.Text.Json;
using System.Text;
using Telegram.Bot.Types;
using OpenAI.Chat;

namespace CarInsuranceTgBot.Services;

public class OpenAiService : IOpenAiService
{
    private readonly HttpClient _httpClient;
    
    // потрібно брати з секретів а не з appsettings
    private readonly string _aiApiKey;
    public OpenAiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("OpenAI");
        _aiApiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAiApiKey не знайдено в конфігурації.");
    }

    public async Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var modelName = "gpt-4o";

        var client = new ChatClient(modelName, _aiApiKey);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("Ти — корисний асистент зі страхування автомобілів."),
            new UserChatMessage(prompt)
        };

        var sb = new StringBuilder();

        var stream = client.CompleteChatStreamingAsync(messages);

        await foreach (var update in stream)
        {
            foreach (var content in update.ContentUpdate)
            {
                sb.Append(content.Text);
            }
        }

        return sb.ToString();
    }
}