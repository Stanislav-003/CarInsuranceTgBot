using CarInsuranceTgBot.Abstractions;
using Mindee.Input;
using Mindee;
using Telegram.Bot;
using Mindee.Http;
using Mindee.Product.Generated;
using Mindee.Parsing.Generated;

namespace CarInsuranceTgBot.Services;

public class MindeeService : IMindeeService
{
    private readonly string _apiKey;

    public MindeeService(IConfiguration configuration)
    {
        _apiKey = configuration["MindeeApi:CarInsuranceBotKey"] ?? throw new InvalidOperationException("Mindee API key не знайдено в конфігурації.");
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

        var documentNumber = SafeGetField(prediction.Fields, "document_number");
        var middleName = SafeGetField(prediction.Fields, "middle_name");
        var name = SafeGetField(prediction.Fields, "name");
        var surname = SafeGetField(prediction.Fields, "surname");
        var validUntil = SafeGetField(prediction.Fields, "valid_until");

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

        var registration_number = SafeGetField(prediction.Fields, "registration_number");
        var date_of_first_registration = SafeGetField(prediction.Fields, "date_of_first_registration");
        var date_of_registration = SafeGetField(prediction.Fields, "date_of_registration");
        var year_of_manufacture = SafeGetField(prediction.Fields, "year_of_manufacture");
        var surname_or_company = SafeGetField(prediction.Fields, "surname_or_company");
        var given_name = SafeGetField(prediction.Fields, "given_name");
        var adressa = SafeGetField(prediction.Fields, "adressa");

        var formattedText = $"Реєстраційний номер №: {registration_number} \n" +
                            $"Дата першої реєстрації: {date_of_first_registration} \n" +
                            $"Дата реєстрації: {date_of_registration} \n" +
                            $"Рік випуску: {year_of_manufacture} \n" +
                            $"Прізвище або організація: {surname_or_company} \n" +
                            $"Ім'я по батькові: {given_name} \n" +
                            $"Адреса: {adressa} \n";


        return formattedText;
    }

    private static string SafeGetField(Dictionary<string, GeneratedFeature> fields, string fieldName)
    {
        return fields.TryGetValue(fieldName, out var field) && field?.AsStringField().Value is string value ? value : "";
    }
}
