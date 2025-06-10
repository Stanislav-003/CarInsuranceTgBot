using CarInsuranceTgBot.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBotServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// ����������� ������������ ������, ������� �������� ����������,
// Telegram api ����������� ���� ��������� ������� Post � BotController �� ���������� �� �������� ���
// ���� ���������� ��������� ����������� ����
await app.UseTelegramWebhookAsync();

app.Run();
