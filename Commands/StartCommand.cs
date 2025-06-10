using CarInsuranceTgBot.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;

namespace CarInsuranceTgBot.Commands;

public class StartCommand : BaseCommand
{
    public override string Name => CommandNames.StartCommand;

    public override async Task ExecuteAsync(Update update, ITelegramBotClient botClient, UserSession session)
    {
        var Username = update.Message!.Chat.Username;
        var ChatId = update.Message.Chat.Id;

        await botClient.SendMessage(ChatId, $"Вітаю {Username}! Я чат-бот, який допоможе Вам придбати автострахування! \n\n" +
                                               "Надішліть будь ласка фотографію свого паспорта");

        // Встановлюємо початковий стан користувача в стейт-машині
        session.State = UserState.WaitingForPassport;
        session.LastCommandName = CommandNames.PhotoHandlerCommand;
    }
}