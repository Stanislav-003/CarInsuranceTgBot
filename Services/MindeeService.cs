using CarInsuranceTgBot.Abstractions;
using Mindee.Input;
using Mindee;
using Telegram.Bot;
using Mindee.Http;
using Mindee.Product.Generated;

namespace CarInsuranceTgBot.Services;

public class MindeeService : IMindeeService
{
    private readonly string _apiKey;

    public MindeeService(IConfiguration configuration)
    {
        _apiKey = configuration["MindeeApi:CarInsuranceBotKey"] ?? throw new InvalidOperationException("Mindee API key не знайдено в конфігурації."); ;
    }

    public async Task<string> ExtractPassportDataAsync(string fileId, ITelegramBotClient botClient, CancellationToken ct)
    {
        var file = await botClient.GetFile(fileId, ct);
        using var memoryStream = new MemoryStream();

        // завантажую у memoryStream файл зображення
        await botClient.DownloadFile(file.FilePath!, memoryStream, ct);
        memoryStream.Position = 0;

        var extension = Path.GetExtension(file.FilePath!) ?? ".jpg"; // default — .jpg

        // створюю тимчасовий файл з унікальним ім'ям
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");

        // записую картинку з бота у тимчасовий файл на диску
        await File.WriteAllBytesAsync(tempFilePath, memoryStream.ToArray(), ct);

        var mindeeClient = new MindeeClient(_apiKey);
        var inputSource = new LocalInputSource(tempFilePath);

        var endpoint = new CustomEndpoint(endpointName: "passport", accountName: "Stanislav", version: "1");

        var response = await mindeeClient.EnqueueAndParseAsync<GeneratedV1>(inputSource, endpoint);

        // видаляю цей тимчасовий файл  
        File.Delete(tempFilePath);

        var prediction = response.Document.Inference.Prediction;

        var documentNumber = prediction.Fields["document_number"].AsStringField().Value;
        var middleName = prediction.Fields["middle_name"].AsStringField().Value;
        var name = prediction.Fields["name"].AsStringField().Value;
        var surname = prediction.Fields["surname"].AsStringField().Value;
        var validUntil = prediction.Fields["valid_until"].AsDateField().Value.ToString();

        var formattedText = $"Документ №: {documentNumber} \n" +
                            $"Ім'я: {name} \n" +
                            $"По батькові: {middleName} \n" +
                            $"Прізвище: {surname} \n" +
                            $"Дійсний до: {validUntil} \n";

        return formattedText;
    }

    public async Task<string> ExtractVehicleDocAsync(string fileId, ITelegramBotClient botClient, CancellationToken ct)
    {
        var file = await botClient.GetFile(fileId, ct);
        using var memoryStream = new MemoryStream();

        // завантажую у memoryStream файл зображення
        await botClient.DownloadFile(file.FilePath!, memoryStream, ct);
        memoryStream.Position = 0;

        var extension = Path.GetExtension(file.FilePath!) ?? ".jpg"; // default — .jpg

        // створюю тимчасовий файл з унікальним ім'ям
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");

        // записую картинку з бота у тимчасовий файл на диску
        await File.WriteAllBytesAsync(tempFilePath, memoryStream.ToArray(), ct);

        var mindeeClient = new MindeeClient(_apiKey);
        var inputSource = new LocalInputSource(tempFilePath);

        var endpoint = new CustomEndpoint(endpointName: "vehicledoc", accountName: "Stanislav", version: "1");

        var response = await mindeeClient.EnqueueAndParseAsync<GeneratedV1>(inputSource, endpoint);

        // видаляю цей тимчасовий файл  
        File.Delete(tempFilePath);

        var prediction = response.Document.Inference.Prediction;

        var registration_number = prediction.Fields["registration_number"].AsStringField().Value;
        var date_of_first_registration = prediction.Fields["date_of_first_registration"].AsDateField().Value.ToString();
        var date_of_registration = prediction.Fields["date_of_registration"].AsDateField().Value.ToString();
        var year_of_manufacture = prediction.Fields["year_of_manufacture"].AsDateField().Value.ToString();
        var surname_or_company = prediction.Fields["surname_or_company"].AsStringField().Value;
        var given_name = prediction.Fields["given_name"].AsStringField().Value;
        var adressa = prediction.Fields["adressa"].AsStringField().Value;


        var formattedText = $"Реєстраційний номер №: {registration_number} \n" +
                            $"Дата першої реєстрації: {date_of_first_registration} \n" +
                            $"Дата реєстрації: {date_of_registration} \n" +
                            $"Рік випуску: {year_of_manufacture} \n" +
                            $"Прізвище або організація: {surname_or_company} \n" +
                            $"Ім'я по батькові: {given_name} \n" +
                            $"Адреса: {adressa} \n";


        return formattedText;
    }
}
