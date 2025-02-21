﻿using ChatBot.API.Interface;
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
                // Lấy thông tin từ CallbackQuery
                var callbackQuery = update.CallbackQuery;
                var callbackData = callbackQuery.Data; // Dữ liệu callback (ví dụ: "feedback", "my_profile", "point")
                long callbackChatId = callbackQuery.Message.Chat.Id;

                // Xử lý dựa trên callbackData
                switch (callbackData)
                {
                    case "feedback":
                        await _botClient.SendTextMessageAsync(
                            chatId: callbackChatId,
                            text: "Please enter your feedback using the /feedback command."
                        );
                        break;

                    case "my_profile":
                        await _botClient.SendTextMessageAsync(
                            chatId: callbackChatId,
                            text: "123"
                        );
                        break;

                    case "point":
                        await _botClient.SendTextMessageAsync(
                            chatId: callbackChatId,
                            text: "123"
                        );
                        break;

                    default:
                        await _botClient.SendTextMessageAsync(
                            chatId: callbackChatId,
                            text: "Invalid option selected."
                        );
                        break;
                }

                // Xác nhận rằng callback đã được xử lý
                await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

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
                // Tạo bàn phím với 3 nút inline
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[] // Hàng 1
                    {
                        InlineKeyboardButton.WithCallbackData("Filters", "filters"), // Nút Suggested
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
                    ⭐ **Please choose an option:** ⭐
                    🔹 Feedback: Đóng góp ý kiến
                    🔹 My Profile: Xem tài khoản
                    🔹 Point: Xem điểm thưởng
                    🔹 Suggested questions: Gợi ý câu hỏi
                    ";
                // Gửi bàn phím đến người dùng
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: messageText,
                    replyMarkup: inlineKeyboard
                );

                _logger.LogInformation("Sent inline keyboard to chat ID: {ChatId}", chatId);
            }
            else if (userMessage.StartsWith("/feedback"))
            {
                // Xử lý phản hồi từ người dùng
                string feedbackMessage = userMessage.Replace("/feedback", "").Trim();
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Thank you for your comment!"
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
