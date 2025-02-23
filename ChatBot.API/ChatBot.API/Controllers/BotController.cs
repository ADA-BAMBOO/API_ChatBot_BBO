using ChatBot.API.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using System.Text.Json;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase;
using TelegramBotBase.Args;


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


            // Xử lý tin nhắn thông thường
            if (update.Message?.Text == null)
            {
                _logger.LogWarning("Message is null");
                return Ok(); // Prevent Telegram from retrying
            }



            var userMessage = update.Message.Text;
            long chatId = update.Message.Chat.Id;

            if (update.CallbackQuery != null)
            {
                string callbackData = update.CallbackQuery.Data;
                chatId = update.CallbackQuery.Message.Chat.Id;

                _logger.LogInformation("🔹 Received CallbackQuery: {Data}", callbackData);

                // Gửi phản hồi để Telegram biết đã nhận callback
                await _botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);

                switch (callbackData)
                {
                    case "filters":
                        await _botClient.SendTextMessageAsync(chatId, "🔍 Here are some suggested questions...");
                        break;
                    case "settings":
                        await _botClient.SendTextMessageAsync(chatId, "⚙ Here are your account settings...");
                        break;
                    case "feedback":
                        await _botClient.SendTextMessageAsync(chatId, "📝 Please provide your feedback...");
                        break;
                    case "point":
                        await _botClient.SendTextMessageAsync(chatId, "🏆 Here is your achievement score...");
                        break;
                    default:
                        await _botClient.SendTextMessageAsync(chatId, "❓ Unknown option selected.");
                        break;
                }

                return Ok();
            }



            if (userMessage == "/start")
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
         {
            InlineKeyboardButton.WithCallbackData("Click me", "test")
        });

                await _botClient.SendTextMessageAsync(update.Message.Chat.Id, "Press the button:", replyMarkup: inlineKeyboard);
            }
            else if (userMessage == "/help")
            {
                // Gọi method public riêng để xử lý lệnh /help
                await HandleHelpCommand(chatId);
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

    #region Method xử lý lệnh /help
    private async Task HandleHelpCommand(long chatId)
    {
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
         new[] // Hàng 1
            {
                InlineKeyboardButton.WithCallbackData("Filter", "filters"), // Nút Suggested
                InlineKeyboardButton.WithCallbackData("Feedback", "feedback"), // Nút Feedback
            },
            new[] // Hàng 2
            {
                InlineKeyboardButton.WithCallbackData("My Profile", "my_profile"), // Nút My Profile
                InlineKeyboardButton.WithCallbackData("Point", "point"), // Nút Point
            }
        });


        // Tạo nội dung tin nhắn với hướng dẫn và khung
        string messageText = @"
🌟 Please choose an option: 
──────────────────────────────
👤 - Settings: Account settings
💡 - Filter: Suggested questions
📝 - Feedback: Give feedback
🎯 - Point: View Achievements
";

        // Gửi bàn phím đến người dùng
        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: messageText,
            replyMarkup: inlineKeyboard
        );

        _logger.LogInformation("Sent inline keyboard to chat ID: {ChatId}", chatId);
    }

    #endregion

    #region Hàm gọi API đến mô hình AI
    private async Task<string> GetResponseFromAI(long chatId, string message)
    {
        using var httpClient = new HttpClient();
        string apiUrl = "http://aitreviet.duckdns.org:8000"; // Thay bằng API của bạn

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
    #endregion
}
