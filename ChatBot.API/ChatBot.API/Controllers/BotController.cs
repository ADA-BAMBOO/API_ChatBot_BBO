using ChatBot.API.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using System.Text.Json;
using System.Text;

namespace ChatBot.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class BotController : ControllerBase
{
    private readonly TelegramBotClient _botClient;
    private readonly ILogger<BotController> _logger;

    public BotController(ILogger<BotController> logger)
    {
        _botClient = new TelegramBotClient("7217303494:AAFECYqX_-SRZLf5CQjgVNlj9_21jKSdMl4");
        _logger = logger;
    }


    [HttpPost]
    public async Task<IActionResult> Post()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var rawBody = await reader.ReadToEndAsync();

            _logger.LogInformation("Raw update: {Update}", rawBody);

            var update = JsonSerializer.Deserialize<Update>(rawBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (update == null || update.Message?.Text == null)
            {
                _logger.LogWarning("Failed to deserialize update or message is null");
                return Ok(); // Prevent Telegram from retrying
            }

            var userMessage = update.Message.Text;
            long chatId = update.Message.Chat.Id;

            if (userMessage == "/start")
            {
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Hello {update.Message.From.FirstName}, welcome to my bot!"
                );
            }
            else
            {
                // Gửi tin nhắn đến API của bạn
                string botReply = await GetResponseFromAI(chatId, userMessage);

                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: botReply
                );
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing update");
            return Ok(); // Prevent Telegram from retrying
        }
    }

    // 🔹 Hàm gọi API đến mô hình AI của bạn
    private async Task<string> GetResponseFromAI(long chatId, string message)
    {
        using var httpClient = new HttpClient();
        string apiUrl = "http://14.236.238.0:8000"; // Thay bằng API của bạn

        var payload = new
        {
            ID = chatId,
            message = message
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync(apiUrl, jsonContent);

        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            return doc.RootElement.GetProperty("answer").GetString(); // Giả sử API trả về { "answer": "câu trả lời" }
        }
        else
        {
            _logger.LogError("API call failed: {StatusCode}", response.StatusCode);
            return "Xin lỗi, tôi không thể xử lý yêu cầu của bạn ngay lúc này.";
        }
    }
}
