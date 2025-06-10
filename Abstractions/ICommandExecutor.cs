using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceTgBot.Abstractions;

public interface ICommandExecutor
{
    Task Execute(Update update, ITelegramBotClient telegramBotClient);
}
