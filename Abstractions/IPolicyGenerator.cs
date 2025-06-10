namespace CarInsuranceTgBot.Abstractions;

public interface IPolicyGenerator
{
    Task<string> GeneratePolicyFileAsync(string extractedData, string price, long chatId, CancellationToken cancellationToken = default);
}
