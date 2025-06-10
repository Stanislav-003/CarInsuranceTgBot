using CarInsuranceTgBot.Abstractions;
using CarInsuranceTgBot.Configuration;
using CarInsuranceTgBot.Extensions;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceTgBot.Commands;

public class PhotoHandlerCommand : BaseCommand
{
    public override string Name => CommandNames.PhotoHandlerCommand;

    private readonly IMindeeService _mindeeService;
    private readonly IPolicyGenerator _policyGenerator;
    private readonly IOptions<InsuranceSettings> _insuranceOptions;

    public PhotoHandlerCommand(
        IMindeeService mindeeService, 
        IPolicyGenerator policyGenerator, 
        IOptions<InsuranceSettings> insuranceOptions)
    {
        _mindeeService = mindeeService;
        _policyGenerator = policyGenerator;
        _insuranceOptions = insuranceOptions;
    }

    public override async Task ExecuteAsync(
        Update update, 
        ITelegramBotClient botClient, 
        UserSession session)
    {
        var chatId = update.Message!.Chat.Id;
        session.LastCommandName = CommandNames.PhotoHandlerCommand;

        switch (session.State)
        {
            case UserState.WaitingForPassport:
                await PhotoHandlerExtension.HandlePassportAsync(update, botClient, session, chatId, _mindeeService);
                break;

            case UserState.WaitingForVehicleDoc:
                await PhotoHandlerExtension.HandleVehicleDocAsync(update, botClient, session, chatId, _mindeeService);
                break;

            case UserState.WaitingForConfirmation:
                await PhotoHandlerExtension.HandleConfirmationAsync(update, botClient, session, chatId, _insuranceOptions.Value);
                break;

            case UserState.WaitingForPriceAgreement:
                await PhotoHandlerExtension.HandlePriceAgreementAsync(update, botClient, session, chatId, _policyGenerator, _insuranceOptions.Value);
                break;
        }
    }
}
