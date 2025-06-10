using CarInsuranceTgBot.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBotServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Автоматично встановлюємо вебхук, вирішуємо проблему лонгполінга,
// Telegram api автоматично буде викликати ендпоїнт Post в BotController та передавати усі необхідні дані
// коли користувач вводитиме повідомлення боту
await app.UseTelegramWebhookAsync();

app.Run();
