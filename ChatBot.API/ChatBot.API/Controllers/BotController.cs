using ChatBot.API.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using System.Text.Json;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace ChatBot.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class BotController : ControllerBase
{
    private readonly TelegramBotClient _botClient;
    private readonly ILogger<BotController> _logger;
    private static Dictionary<long, UserFeedbackState> _userFeedbackStates = new();

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

            // Xử lý CallbackQuery (khi người dùng nhấn vào nút inline)
            if (update.CallbackQuery != null)
            {
                _logger.LogInformation("Received CallbackQuery: {CallbackQueryData}", update.CallbackQuery.Data);
                await HandleCallbackQuery(update.CallbackQuery);
                return Ok();
            }

            // Xử lý tin nhắn thông thường
            if (update.Message?.Text == null)
            {
                _logger.LogWarning("Message is null");
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
            else if (userMessage == "/help")
            {
                // Gọi method public riêng để xử lý lệnh /help
                await HandleHelpCommand(chatId);
            }
            else if (userMessage.StartsWith("/feedback"))
            {
                // Yêu cầu người dùng nhập nội dung phản hồi
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Please enter your feedback:"
                );

                // Lưu trạng thái người dùng
                _userFeedbackStates[chatId] = new UserFeedbackState();
            }
            else if (_userFeedbackStates.ContainsKey(chatId) && string.IsNullOrEmpty(_userFeedbackStates[chatId].FeedbackText))
            {
                // Lưu nội dung phản hồi
                _userFeedbackStates[chatId].FeedbackText = userMessage;

                // Hiển thị bàn phím inline để chọn đánh giá
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("1", "rating_1"),
                        InlineKeyboardButton.WithCallbackData("2", "rating_2"),
                        InlineKeyboardButton.WithCallbackData("3", "rating_3"),
                        InlineKeyboardButton.WithCallbackData("4", "rating_4"),
                        InlineKeyboardButton.WithCallbackData("5", "rating_5"),
                    }
                });

                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Please rate your experience (1-5):",
                    replyMarkup: inlineKeyboard
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

    // 🔹 Method xử lý lệnh /help
    private async Task HandleHelpCommand(long chatId)
    {
        // Tạo bàn phím với 3 nút inline
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
👤 - My Profile: Account settings
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


    // 🔹 Method xử lý CallbackQuery

    private async Task HandleCallbackQuery(CallbackQuery callbackQuery)
    {
        var callbackData = callbackQuery.Data;
        long callbackChatId = callbackQuery.Message.Chat.Id;

        _logger.LogInformation("Handling CallbackQuery: {CallbackData}", callbackData);

        if (callbackData.StartsWith("rating_"))
        {
            // Lưu đánh giá
            int rating = int.Parse(callbackData.Replace("rating_", ""));
            _userFeedbackStates[callbackChatId].Rating = rating;

            // Hiển thị nút Submit
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Submit", "submit_feedback"),
                }
            });

            await _botClient.SendTextMessageAsync(
                chatId: callbackChatId,
                text: $"You selected rating: {rating}. Press Submit to confirm.",
                replyMarkup: inlineKeyboard
            );
        }
        else if (callbackData == "submit_feedback")
        {
            // Xử lý khi người dùng nhấn Submit
            var feedbackState = _userFeedbackStates[callbackChatId];
            string feedbackText = feedbackState.FeedbackText;
            int rating = feedbackState.Rating;

            // Gửi phản hồi hoặc lưu vào cơ sở dữ liệu
            await _botClient.SendTextMessageAsync(
                chatId: callbackChatId,
                text: $"Thank you for your feedback!\nFeedback: {feedbackText}\nRating: {rating}"
            );

            // Xóa trạng thái người dùng
            _userFeedbackStates.Remove(callbackChatId);
        }

        // Xác nhận rằng callback đã được xử lý
        await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
    }
    public class UserFeedbackState
    {
        public string FeedbackText { get; set; }
        public int Rating { get; set; }
    }

    // 🔹 Hàm gọi API đến mô hình AI
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
}
