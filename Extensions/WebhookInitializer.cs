using CarInsuranceTgBot.Configuration;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace CarInsuranceTgBot.Extensions;

public static class WebhookInitializer
{
    public static async Task UseTelegramWebhookAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
        var config = scope.ServiceProvider.GetRequiredService<IOptions<BotConfiguration>>().Value;

        var webhookUrl = config.BotWebhookUrl.AbsoluteUri;

        await botClient.SetWebhook(
            url: webhookUrl,
            allowedUpdates: [],
            secretToken: config.SecretToken,
            cancellationToken: CancellationToken.None);

        Console.WriteLine($"Webhook set to {webhookUrl}");
    }
}
