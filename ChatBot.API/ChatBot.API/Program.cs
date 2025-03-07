using ChatBot.API.Controllers;
using ChatBot.API.Interface;
using ChatBot.API.Models;
using ChatBot.API.Reponsitory;
using ChatBot.API.Handle;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình JSON cho controllers
builder.Services
    .AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        options.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
        options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
    });

builder.Services.AddControllers();

// Cấu hình Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cấu hình database (PostgreSQL hoặc khác)
builder.Services.AddDbContext<YourDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("dbConnection"))
);

// Cấu hình UnitOfWork cho Repository Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWorkReponsitory>();

// Đăng ký TelegramBotClient từ appsettings.json
builder.Services.AddSingleton<ITelegramBotClient>(sp =>
    new TelegramBotClient(builder.Configuration["TelegramBot:Token"]));

// Thêm logging
builder.Logging.AddConsole();

// Đăng ký BotController
builder.Services.AddScoped<BotController>();

// Đăng ký BotCommandHandler
builder.Services.AddScoped<BotCommandHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Đặt webhook và cập nhật lệnh bot khi ứng dụng khởi động
app.Lifetime.ApplicationStarted.Register(async () =>
{
    using (var scope = app.Services.CreateScope())
    {
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
        var botCommandHandler = scope.ServiceProvider.GetRequiredService<BotCommandHandler>();
        var cancellationTokenSource = new CancellationTokenSource();

        // Đặt webhook
        var webhookUrl = builder.Configuration["Webhook:Url"] ?? "https://b24f-171-227-32-223.ngrok-free.app/api/bot";
        await botClient.SetWebhookAsync(webhookUrl);

        // Cập nhật lệnh bot
        await botCommandHandler.SetBotCommandsAsync(cancellationTokenSource.Token);
    }
});

app.Run();