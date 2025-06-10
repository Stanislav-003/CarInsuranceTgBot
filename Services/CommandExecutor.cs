using CarInsuranceTgBot.Abstractions;
using CarInsuranceTgBot.Commands;
using Microsoft.AspNetCore.Session;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceTgBot.Services;

public class CommandExecutor : ICommandExecutor
{
    // Симуляція бд де long - це id користувача, UserSession - додаткова інформація
    private readonly IUserSessionStore _userSessionStore;
    private readonly IOpenAiService _openAiService;

    // Фабрика, яка зручно викликає команди
    private readonly ICommandFactory _commandFactory;

    public CommandExecutor(
        ICommandFactory commandFactory,
        IUserSessionStore userSessionStore,
        IOpenAiService openAiService)
    {
        _commandFactory = commandFactory;
        _userSessionStore = userSessionStore;
        _openAiService = openAiService;
    }

    public async Task Execute(Update update, ITelegramBotClient telegramBot)
    {
        if (update.Message == null || update.Message.From == null)
            return;

        // Id користувача, який надіслав повідомлення боту
        long userId = update.Message!.From!.Id;

        // Отримуємо інформаціє про користувача
        var session = _userSessionStore.GetOrCreateSession(userId);

        // Текст який ввів користувач боту
        var messageText = update.Message.Text;

        // Якщо повідомлення починається на "/" виконуємо відповідну команду з фабрики
        if (!string.IsNullOrEmpty(messageText) && messageText.StartsWith("/"))
        {
            // Отримуємо команду з фабрики за текстом повідомлення
            var command = _commandFactory.GetCommand(messageText);

            // Якщо команда не знайдена, то повідомляємо користувача про помилку
            if (command == null)
            {
                await telegramBot.SendMessage(userId, "Невідома команда. Спробуйте ще раз /start.");
                return;
            }

            // Виконуємо команду
            await command.ExecuteAsync(update, telegramBot, session);
            return;
        }

        // Якщо повідомлення відмінне від "/" виконуємо останню команду стейт-машини
        if (!string.IsNullOrEmpty(session.LastCommandName))
        {
            // Отримуємо останню команду, яка була виконана
            var command = _commandFactory.GetCommand(session.LastCommandName);

            // Виконуємо команду
            if (command != null)
            {
                await command.ExecuteAsync(update, telegramBot, session);
                return;
            }
        }

        //try
        //{
        //    // Відповідь через OpenAI, якщо не вдалося розпізнати команду
        //    if (!string.IsNullOrWhiteSpace(messageText))
        //    {
        //        var response = await _openAiService.AskAsync(messageText);
        //        await telegramBot.SendMessage(userId, response);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine(ex.Message);
        //    throw;
        //}
    }
}
