namespace CarInsuranceTgBot.Abstractions;

public interface IOpenAiService
{
    Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default);
}
