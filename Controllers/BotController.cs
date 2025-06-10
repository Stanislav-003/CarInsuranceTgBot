using CarInsuranceTgBot.Abstractions;
using CarInsuranceTgBot.Configuration;
using CarInsuranceTgBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceTgBot.Controllers;

[ApiController]
[Route("[controller]")]
public class BotController : ControllerBase
{
    private readonly IOptions<BotConfiguration> _config;
    private readonly ICommandExecutor _commandExecutor;

    public BotController(
        IOptions<BotConfiguration> config, 
        ICommandExecutor commandExecutor)
    {
        _config = config;
        _commandExecutor = commandExecutor;
    }

    [HttpGet("setWebhook")]
    public async Task<string> SetWebHook(
        [FromServices] ITelegramBotClient bot,
        CancellationToken ct)
    {
        var webhookUrl = _config.Value.BotWebhookUrl.AbsoluteUri;

        await bot.SetWebhook(
            webhookUrl,
            allowedUpdates: [],
            secretToken: _config.Value.SecretToken,
            cancellationToken: ct);

        return $"Webhook set to {webhookUrl}";
    }

    [HttpPost]
    public async Task<IActionResult> Post(
        [FromBody] Update update,
        [FromServices] ITelegramBotClient bot,
        CancellationToken ct)
    {
        if (Request.Headers["X-Telegram-Bot-Api-Secret-Token"] != _config.Value.SecretToken)
            return Forbid();

        if (update.Message?.From == null)
            return BadRequest("Invalid update format.");

        try
        {
            await _commandExecutor.Execute(update, bot);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occurred: {e.Message}");
            return Ok();
        }

        return Ok();
    }
}
