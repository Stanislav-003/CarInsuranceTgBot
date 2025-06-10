using CarInsuranceTgBot.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceTgBot.Commands;

public abstract class BaseCommand
{
    public abstract string Name { get; }
    public abstract Task ExecuteAsync(Update update, ITelegramBotClient botClient, UserSession session);
}
