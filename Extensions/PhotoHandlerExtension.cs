using CarInsuranceTgBot.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot;
using CarInsuranceTgBot.Configuration;
using Microsoft.Extensions.Options;

namespace CarInsuranceTgBot.Extensions;

public static class PhotoHandlerExtension
{
    public static async Task HandlePassportAsync(
        Update update, 
        ITelegramBotClient botClient, 
        UserSession session, 
        long chatId, 
        IMindeeService mindeeService)
    {
        if (update.Message?.Photo == null)
        {
            await botClient.SendMessage(chatId, "Надішліть фотографію свого паспорта");
            return;
        }

        var fileId = update.Message.Photo.Last().FileId;
        session.PassportImageFileId = fileId;

        var waitingMessage = await botClient.SendMessage(chatId, "Зачекайте, обробляємо документ...");

        try
        {
            session.PassportData = await mindeeService.ExtractPassportDataAsync(fileId, botClient, CancellationToken.None);
        }
        catch (Exception ex)
        {
            await botClient.SendMessage(chatId, $"Не вдалося розпізнати документ спробуйте ще раз!");
            session.State = UserState.WaitingForPassport;
            await botClient.DeleteMessage(chatId, waitingMessage.MessageId);
            Console.WriteLine(ex.Message);
        }

        await botClient.DeleteMessage(chatId, waitingMessage.MessageId);

        session.State = UserState.WaitingForVehicleDoc;

        await botClient.SendMessage(chatId, "Тепер надішліть фото документа транспортного засобу");
    }

    public static async Task HandleVehicleDocAsync(
        Update update, 
        ITelegramBotClient botClient, 
        UserSession session, 
        long chatId, 
        IMindeeService mindeeService)
    {
        if (update.Message?.Photo == null)
        {
            await botClient.SendMessage(chatId, "Надішліть фото документа транспортного засобу");
            return;
        }

        var fileId = update.Message.Photo.Last().FileId;
        session.VehicleDocImageFileId = fileId;

        var waitingMessage = await botClient.SendMessage(chatId, "Зачекайте, будь ласка, триває обробка зображення...");

        try
        {
            session.VehicleDocData = await mindeeService.ExtractVehicleDocAsync(fileId, botClient, CancellationToken.None);
        }
        catch (Exception ex)
        {
            await botClient.SendMessage(chatId, $"Не вдалося розпізнати документ спробуйте ще раз!");
            session.State = UserState.WaitingForVehicleDoc;
            await botClient.DeleteMessage(chatId, waitingMessage.MessageId);
            Console.WriteLine(ex.Message);
        }

        session.State = UserState.WaitingForConfirmation;

        await botClient.DeleteMessage(chatId, waitingMessage.MessageId);

        await botClient.SendMessage(chatId, $"Ось що ми розпізнали з ваших документів:\n\n{session.FullExtractedData}\n\nВсе правильно? (так/ні)");
    }

    public static async Task HandleConfirmationAsync(
        Update update, 
        ITelegramBotClient botClient, 
        UserSession session, 
        long chatId,
        InsuranceSettings insuranceOptions)
    {
        var response = Normalize(update.Message?.Text);

        if (response == insuranceOptions.ConfirmationYes)
        {
            session.State = UserState.WaitingForPriceAgreement;
            await botClient.SendMessage(chatId, $"Вартість страхування: {insuranceOptions.InsurancePrice}. Ви згодні? (так/ні)");
            return;
        }

        if (response == insuranceOptions.ConfirmationNo)
        {
            session.State = UserState.WaitingForPassport;
            await botClient.SendMessage(chatId, $"Добре, надішліть фото паспорта ще раз.");
            return;
        }

        if (response != insuranceOptions.ConfirmationYes && response != insuranceOptions.ConfirmationNo)
        {
            session.State = UserState.WaitingForConfirmation;
            await botClient.SendMessage(chatId, "Не зрозумів, Все правильно? (так/ні)");
            return;
        }
    }

    public static async Task HandlePriceAgreementAsync(
        Update update, 
        ITelegramBotClient botClient, 
        UserSession session, 
        long chatId, 
        IPolicyGenerator policyGenerator, 
        InsuranceSettings insuranceOptions)
    {
        var response = Normalize(update.Message?.Text);

        if (response == insuranceOptions.ConfirmationYes)
        {
            session.State = UserState.PolicyIssued;

            var waitingMessage = await botClient.SendMessage(chatId, "Зачекайте, формуємо ваш страховий поліс...");

            try
            {
                string filePath = await policyGenerator.GeneratePolicyFileAsync(session.FullExtractedData!, insuranceOptions.InsurancePrice, chatId);
                using FileStream stream = File.OpenRead(filePath);

                await botClient.DeleteMessage(chatId, waitingMessage.MessageId);

                await botClient.SendDocument(chatId, new InputFileStream(stream, Path.GetFileName(filePath)),
                    caption: "Ось ваш страховий поліс. Дякуємо за покупку! \n " +
                             "Для повторного виклику бота скористуйтеся командою /start");
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(chatId, "Вибачте, сталася помилка при формуванні поліса. Спробуйте пізніше.");
                session.State = UserState.WaitingForPriceAgreement;
                await botClient.DeleteMessage(chatId, waitingMessage.MessageId);
                Console.WriteLine(ex.Message);
            }

            return;
        }

        if (response == insuranceOptions.ConfirmationNo)
        {
            session.State = UserState.WaitingForPriceAgreement;
            await botClient.SendMessage(chatId, $"На жаль, ціна фіксована — {insuranceOptions.InsurancePrice}((( \nВи згодні? так/ні");
            return;
        }

        if (response != insuranceOptions.ConfirmationYes && response != insuranceOptions.ConfirmationNo)
        {
            session.State = UserState.WaitingForPriceAgreement;
            await botClient.SendMessage(chatId, "Не зрозумів, Ви згодні? (так/ні)");
            return;
        }
    }

    private static string Normalize(string? input) => input?.Trim().ToLowerInvariant() ?? string.Empty;
}