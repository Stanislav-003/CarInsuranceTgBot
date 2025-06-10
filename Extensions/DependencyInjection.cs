using CarInsuranceTgBot.Abstractions;
using CarInsuranceTgBot.Commands;
using CarInsuranceTgBot.Configuration;
using CarInsuranceTgBot.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using Telegram.Bot;

namespace CarInsuranceTgBot.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddBotServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Конфігурування бота. Отримуємо налаштування бота з конфігурації
        services.Configure<BotConfiguration>(configuration.GetSection("BotConfiguration"));

        services.Configure<InsuranceSettings>(configuration.GetSection("InsuranceSettings"));

        // Налаштування вебхука
        services.AddHttpClient("tgwebhook")
            .RemoveAllLoggers()
            .AddTypedClient<ITelegramBotClient>(httpClient =>
                new TelegramBotClient(configuration.GetSection("BotConfiguration").Get<BotConfiguration>()!.BotToken, httpClient));

        // Реєстрація сесії яка імітує базу даних
        services.AddSingleton<Dictionary<long, UserSession>>();

        // Реєстрація фабрик
        services.AddSingleton<IUserSessionStore, InMemoryUserSessionStore>();
        services.AddSingleton<ICommandExecutor, CommandExecutor>();
        services.AddSingleton<ICommandFactory, CommandFactory>();
        
        // Реєстрація команд
        services.AddSingleton<StartCommand>();
        services.AddSingleton<PhotoHandlerCommand>();

        // Сервіс, який імітує запит на стороннє апі для парсингу картинок
        // Реєструю їх як сервіси а не статичні класи оскільки вони імітують взаємодію з зовнішніми апі
        services.AddSingleton<IMindeeService, MindeeService>();

        // Сервіс, який створює страховий поліс у вигляді документа на основі отриманих даних від Mindee api
        services.AddSingleton<IPolicyGenerator, PolicyGenerator>();

        services.AddSingleton<IOpenAiService, OpenAiService>();

        return services;
    }
}
