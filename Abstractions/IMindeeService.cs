using Telegram.Bot;

namespace CarInsuranceTgBot.Abstractions;

public interface IMindeeService
{
    Task<string> ExtractPassportDataAsync(string fileId, ITelegramBotClient botClient, CancellationToken cancellationToken = default);
    Task<string> ExtractVehicleDocAsync(string fileId, ITelegramBotClient botClient, CancellationToken cancellationToken = default);

}
