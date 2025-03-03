using ChatBot.API.Controllers;
using ChatBot.API.Interface;
using ChatBot.API.Models;
using ChatBot.API.Reponsitory;
//using ChatBot.API.Handle;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Telegram.Bot;


var builder = WebApplication.CreateBuilder(args);

// ?? C?u hình JSON cho controllers
builder.Services
    .AddControllers()
    .AddNewtonsoftJson(options => {
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        options.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
        options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
    });

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ?? Cấu hình database (PostgreSQL ho?c khác)
builder.Services.AddDbContext<YourDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("dbConnection"))
    );

// Cấu hình UnitOfWork cho Repository Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWorkReponsitory>();

// Đănng ký TelegramBotClient t? appsettings.json
builder.Services.AddSingleton<ITelegramBotClient>(sp =>
    new TelegramBotClient(builder.Configuration["TelegramBot:Token"]));

// Thêm logging
builder.Logging.AddConsole();

// Đăng ký BotController (thay vì HostedService MyBot)
builder.Services.AddScoped<BotController>();

// Add the bot as a hosted service
//builder.Services.AddHostedService<MyBot>();

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

// Đặt webhook khi ứng dụng khởi động
app.Lifetime.ApplicationStarted.Register(async () =>
{
    var botClient = app.Services.GetRequiredService<ITelegramBotClient>();
    var webhookUrl = builder.Configuration["Webhook:Url"] ?? "https://e2bb-171-254-208-205.ngrok-free.app/api/bot"; // Thay bằng URL công khai của bạn
    await botClient.SetWebhookAsync(webhookUrl);
});

app.Run();
