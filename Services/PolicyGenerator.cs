using CarInsuranceTgBot.Abstractions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CarInsuranceTgBot.Services;

public class PolicyGenerator : IPolicyGenerator
{
    private const string DirectoryPath = "GeneratedPolicies";

    // Метод який підготовлює інформацію страхового поліса користувача у PDF форматі
    public async Task<string> GeneratePolicyFileAsync(string extractedData, string price, long chatId, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(DirectoryPath))
            Directory.CreateDirectory(DirectoryPath);

        var fileName = $"policy_{chatId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
        var filePath = Path.Combine(DirectoryPath, fileName);

        // генерую просто рандомне значення
        var policyNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        var content = $"""
        {extractedData}

        Сума страхування: {price}
        Поліс видано успішно.
        """;

        // Створюємо PDF-документ
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(14));

                page.Header().Text("СТРАХОВИЙ ПОЛІС").FontSize(24).Bold().AlignCenter();

                page.Content().Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Text($"Дата: {timestamp} UTC");
                    col.Item().Text($"Номер поліса: {policyNumber}");
                    col.Item().Text("");
                    col.Item().Text(content);
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Car Insurance Bot © ").SemiBold();
                    txt.Span(DateTime.UtcNow.Year.ToString());
                });
            });
        });

        // Зберігаємо PDF у файл
        QuestPDF.Settings.License = LicenseType.Community;
        await Task.Run(() => document.GeneratePdf(filePath), cancellationToken);

        return filePath;
    }
}
