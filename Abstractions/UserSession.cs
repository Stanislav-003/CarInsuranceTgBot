using CarInsuranceTgBot.Commands;

namespace CarInsuranceTgBot.Abstractions;

public class UserSession
{
    public UserState State { get; set; } = UserState.None;
    public string? LastCommandName { get; set; }
    public string? PassportImageFileId { get; set; }
    public string? VehicleDocImageFileId { get; set; }
    public string? PassportData { get; set; }
    public string? VehicleDocData { get; set; }

    public string FullExtractedData => $"{PassportData}\n\n{VehicleDocData}";
}
