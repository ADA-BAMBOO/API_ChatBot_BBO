using ChatBot.API.Interface;
using ChatBot.API.Models;
using ChatBot.API.Reponsitory;
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

// ?? C?u hình database (PostgreSQL ho?c khác)
builder.Services.AddDbContext<YourDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("dbConnection"))
    );

// ?? C?u hình UnitOfWork cho Repository Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWorkReponsitory>();

// ?? ??ng ký TelegramBotClient t? appsettings.json
builder.Services.AddSingleton<ITelegramBotClient>(sp =>
    new TelegramBotClient(builder.Configuration["TelegramBot:Token"]));

// ?? Thêm logging
builder.Logging.AddConsole();

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

app.Run();
