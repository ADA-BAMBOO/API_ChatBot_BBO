﻿using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Collections.Generic;

namespace ChatBot.API.Handle;

public class MyBot : IHostedService
{
    #region Khai báo contructor
    private readonly TelegramBotClient _botClient;
    private readonly ILogger<MyBot> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Dictionary<long, (string State, string Comment)> _feedbackState; // Store user state and comment

    public MyBot(ILogger<MyBot> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _botClient = new TelegramBotClient("7734778997:AAE0KtrSjeipZAX6-pM_yUxq6rP6N8z2lcs");
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
        _serviceScopeFactory = serviceScopeFactory;
        _feedbackState = new Dictionary<long, (string, string)>(); // Initialize feedback state storage
    }
    #endregion

    #region Start bot & Allow Callback
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery },
            ThrowPendingUpdates = true
        };

        _botClient.StartReceiving(
            updateHandler: async (botClient, update, ct) => await HandleUpdateAsync(botClient, update, ct),
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: _cancellationTokenSource.Token
        );

        var me = await _botClient.GetMeAsync(cancellationToken);
        _logger.LogInformation("Bot started: @{BotUsername}", me.Username);
    }
    #endregion

    #region Xử lý Logic bot
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message != null)
            {
                var chatId = update.Message.Chat.Id;

                // Xử lý state
                if (_feedbackState.ContainsKey(chatId) && update.Message.Text != null)
                {
                    var (state, _) = _feedbackState[chatId];
                    if (state == "awaiting_comment")
                    {
                        _feedbackState[chatId] = ("awaiting_rating", update.Message.Text);
                        await SendRatingButtons(botClient, chatId, cancellationToken);
                        return;
                    }
                    else if (state == "awaiting_onchainid")
                    {
                        await UpdateOnchainId(botClient, chatId, update.Message.Text);
                        return;
                    }
                }

                if (update.Message.Text != null)
                {
                    var messageText = update.Message.Text;
                    _logger.LogInformation("Received message: {Message}", messageText);

                    if (messageText == "/start")
                    {
                        #region Create new user
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                            var existingUser = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId);

                            if (existingUser == null)
                            {
                                var newUser = new BboUser
                                {
                                    Telegramid = (int)chatId,
                                    Username = update.Message.From?.Username,
                                    Joindate = DateTime.Now,
                                    Lastactive = DateTime.Now,
                                    Isactive = true,
                                    Roleid = 3
                                };

                                await unitOfWork.userReponsitory.AddEntity(newUser);
                                await unitOfWork.CompleteAsync();
                                _logger.LogInformation("Created new user with Telegram ID: {TelegramId}", chatId);
                            }
                        }
                        #endregion

                        #region Create inline keyboard
                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new[] { InlineKeyboardButton.WithCallbackData("👤 Settings", "settings"), InlineKeyboardButton.WithCallbackData("💡 Filter", "filter") },
                            new[] { InlineKeyboardButton.WithCallbackData("📝 Feedback", "feedback"), InlineKeyboardButton.WithCallbackData("🎯 Point", "point") },
                        });
                        #endregion

                        #region Send welcome message
                        var welcomeMessage =
                            $"Hi {update.Message.From?.Username}, Welcome to ADA-BBO Bot!\n\n" +
                            "🌟 Please select an option:\n" +
                            "────────────────────────────────\n" +
                            "👤 - Settings: Account Settings\n" +
                            "💡 - Filters: Recommended Questions\n" +
                            "📝 - Feedback: Submit Feedback\n" +
                            "🎯 - Score: View Achievements\n\n" +
                            "Or you can use the following commands:\n" +
                            "────────────────────────────────\n" +
                            "💡 - /help - Show available commands\n" +
                            "👤 - /settings - Account settings\n" +
                            "💡 - /filter - Suggested questions\n" +
                            "📝 - /feedback - Return feedback\n" +
                            "🎯 - /point - View achievements";

                        await botClient.SendTextMessageAsync(chatId, welcomeMessage, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
                        #endregion
                        return;
                    }

                    string responseMessage = messageText switch
                    {
                        "/help" => await GetHelpMessage(botClient, chatId, cancellationToken),
                        "/settings" => await GetSettingsMessage(botClient, chatId, cancellationToken),
                        "/filter" => "🔍 Filter Options Menu",
                        "/feedback" => await StartFeedbackProcess(botClient, chatId, cancellationToken),
                        "/point" => "🎯 Your Achievement Points",
                        _ => await GetResponseFromAI(chatId, messageText)
                    };

                    if (messageText != "/help" && messageText != "/feedback" && messageText != "/settings")
                    {
                        await botClient.SendTextMessageAsync(chatId, responseMessage, cancellationToken: cancellationToken);
                    }
                }
            }
            else if (update.CallbackQuery != null)
            {
                await HandleCallbackQuery(botClient, update.CallbackQuery);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling update");
        }
    }
    #endregion

    #region Settings Process
    private async Task<string> GetSettingsMessage(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var user = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId);

            if (user == null)
            {
                return "User not found. Please use /start to register.";
            }

            var role = await unitOfWork.roleReponsitory.GetAsync(user.Roleid ?? 0);
            var roleName = role?.Rolename ?? "User";

            var settingsMessage = "Account Information:\n" +
                                "─────────────────────────────\n" +
                                $"Username: {user.Username ?? "Not set"}\n" +
                                $"TelegramId: {user.Telegramid}\n" +
                                $"Joindate: {user.Joindate?.ToString("dd/MM/yyyy") ?? "N/A"}\n" +
                                $"Is active: {(user.Isactive == true ? "Active" : "Inactive")}\n" +
                                $"Role: {roleName}\n" +
                                $"Onchain ID: {user.Onchainid ?? "Not set"}";

            var inlineKeyboard = new InlineKeyboardMarkup(
                InlineKeyboardButton.WithCallbackData("Update Onchain ID", "update_onchain")
            );

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: settingsMessage,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
            );

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching settings for Telegram ID: {TelegramId}", chatId);
            return "Error retrieving your account information.";
        }
    }

    private async Task<string> StartUpdateOnchainProcess(ITelegramBotClient botClient, long chatId)
    {
        _feedbackState[chatId] = ("awaiting_onchainid", string.Empty);
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Please enter your new Onchain ID:",
            cancellationToken: _cancellationTokenSource.Token
        );
        return string.Empty;
    }

    private async Task UpdateOnchainId(ITelegramBotClient botClient, long chatId, string newOnchainId)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var user = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId);

            if (user != null)
            {
                user.Onchainid = newOnchainId;
                await unitOfWork.userReponsitory.UpdateEntity(user);
                await unitOfWork.CompleteAsync();

                _feedbackState.Remove(chatId);
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Onchain ID updated successfully! Use /settings to view your updated information.",
                    cancellationToken: _cancellationTokenSource.Token
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Onchain ID for Telegram ID: {TelegramId}", chatId);
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Error updating Onchain ID. Please try again.",
                cancellationToken: _cancellationTokenSource.Token
            );
        }
    }
    #endregion

    #region Feedback Process
    private async Task<string> StartFeedbackProcess(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        _feedbackState[chatId] = ("awaiting_comment", string.Empty);
        await botClient.SendTextMessageAsync(chatId, "Please enter your feedback:", cancellationToken: cancellationToken);
        return string.Empty;
    }

    private async Task SendRatingButtons(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var ratingKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("1", "rate_1"), InlineKeyboardButton.WithCallbackData("2", "rate_2"), InlineKeyboardButton.WithCallbackData("3", "rate_3") },
            new[] { InlineKeyboardButton.WithCallbackData("4", "rate_4"), InlineKeyboardButton.WithCallbackData("5", "rate_5"), InlineKeyboardButton.WithCallbackData("6", "rate_6") }
        });

        var messageText = "Please select your satisfaction level:\n" +
                         "────────────────────────────────\n" +
                         "• Button 1 ➡️ Dissatisfied\n" +
                         "• Button 2 ➡️ Slightly disappointed\n" +
                         "• Button 3 ➡️ Average\n" +
                         "• Button 4 ➡️ Satisfied\n" +
                         "• Button 5 ➡️ Very satisfied\n" +
                         "• Button 6 ➡️ Skip";

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: messageText,
            replyMarkup: ratingKeyboard,
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleFeedbackRating(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var chatId = callbackQuery.Message.Chat.Id;
        var callbackData = callbackQuery.Data;

        if (_feedbackState.ContainsKey(chatId) && _feedbackState[chatId].State == "awaiting_rating" && callbackData.StartsWith("rate_"))
        {
            int rating = int.Parse(callbackData.Split('_')[1]);
            string comment = _feedbackState[chatId].Comment;

            await SaveFeedback(chatId, rating, comment);
            _feedbackState.Remove(chatId);

            await botClient.SendTextMessageAsync(chatId, "Thank you for your feedback! 💖");
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
    }

    private async Task SaveFeedback(long telegramId, int rating, string comment)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var feedback = new BboFeedback
            {
                Userid = (int)telegramId,
                Rating = rating,
                Comment = comment,
                Createdat = DateTime.Now
            };

            await unitOfWork.feedbackReponsitory.AddEntity(feedback);
            await unitOfWork.CompleteAsync();
            _logger.LogInformation("Feedback saved for Telegram ID: {TelegramId}, Rating: {Rating}", telegramId, rating);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving feedback for Telegram ID: {TelegramId}", telegramId);
        }
    }
    #endregion

    #region Gọi lệnh Help Message
    private async Task<string> GetHelpMessage(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var helpMessage = "Available commands:\n" +
                         "────────────────────────────────\n" +
                         "💡 - /help - Show available commands\n" +
                         "👤 - /settings - Account settings\n" +
                         "💡 - /filter - Suggested questions\n" +
                         "📝 - /feedback - Return feedback\n" +
                         "🎯 - /point - View achievements";

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("👤 Settings", "settings"), InlineKeyboardButton.WithCallbackData("💡 Filter", "filter") },
            new[] { InlineKeyboardButton.WithCallbackData("📝 Feedback", "feedback"), InlineKeyboardButton.WithCallbackData("🎯 Point", "point") },
        });

        await botClient.SendTextMessageAsync(chatId, helpMessage, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
        return helpMessage;
    }
    #endregion

    #region Add callback query handler
    private async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var chatId = callbackQuery.Message.Chat.Id;
        var response = callbackQuery.Data switch
        {
            "settings" => await GetSettingsMessage(botClient, chatId, _cancellationTokenSource.Token),
            "filter" => "🔍 Filter Options Menu",
            "feedback" => await StartFeedbackProcess(botClient, chatId, _cancellationTokenSource.Token),
            "point" => "🎯 Your Achievement Points",
            "update_onchain" => await StartUpdateOnchainProcess(botClient, chatId),
            "rate_1" or "rate_2" or "rate_3" or "rate_4" or "rate_5" or "rate_6" =>
                await HandleFeedbackRating(botClient, callbackQuery).ContinueWith(_ => string.Empty),
            _ => "Invalid option"
        };

        if (callbackQuery.Data != "feedback" &&
            !callbackQuery.Data.StartsWith("rate_") &&
            callbackQuery.Data != "settings" &&
            callbackQuery.Data != "update_onchain")
        {
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            await botClient.SendTextMessageAsync(chatId, response);
        }
        else if (callbackQuery.Data == "update_onchain" || callbackQuery.Data == "settings")
        {
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
    }
    #endregion

    #region HandlePollingErrorAsync
    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Polling error occurred");
        return Task.CompletedTask;
    }
    #endregion

    #region Gọi API AI Model
    private async Task<string> GetResponseFromAI(long chatId, string message)
    {
        var loadingMessage = await _botClient.SendTextMessageAsync(chatId, "Thinking...", cancellationToken: _cancellationTokenSource.Token);
        var cts = new CancellationTokenSource();
        var loadingTask = Task.Run(async () =>
        {
            string[] dots = new[] { "Thinking", "Thinking.", "Thinking..", "Thinking..." };
            int index = 0;
            while (!cts.IsCancellationRequested)
            {
                await _botClient.EditMessageTextAsync(chatId, loadingMessage.MessageId, dots[index], cancellationToken: _cancellationTokenSource.Token);
                index = (index + 1) % dots.Length;
                await Task.Delay(500, _cancellationTokenSource.Token);
            }
        }, _cancellationTokenSource.Token);

        try
        {
            using var httpClient = new HttpClient();
            string apiUrl = "http://aitreviet.duckdns.org:8000";
            var payload = new { ID = chatId, message = message };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(apiUrl, jsonContent);
            string aiResponse;
            decimal? responseTime = null;

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;
                aiResponse = root.GetProperty("answer").GetString() ?? "Sorry, I couldn't process your request.";
                responseTime = root.TryGetProperty("time", out var timeElement) ? timeElement.GetDecimal() : null;
            }
            else
            {
                _logger.LogError("API call failed with status code: {StatusCode}", response.StatusCode);
                aiResponse = "Sorry, I couldn't process your request at this time.";
            }

            cts.Cancel();
            try { await loadingTask; } catch (TaskCanceledException) { }
            await _botClient.DeleteMessageAsync(chatId, loadingMessage.MessageId, cancellationToken: _cancellationTokenSource.Token);
            await CreateChatHistoryAsync(chatId, message, aiResponse, responseTime);

            return aiResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetResponseFromAI");
            cts.Cancel();
            try { await loadingTask; } catch (TaskCanceledException) { }
            await _botClient.DeleteMessageAsync(chatId, loadingMessage.MessageId, cancellationToken: _cancellationTokenSource.Token);
            await CreateChatHistoryAsync(chatId, message, null, null);
            return "Sorry, I couldn't process your request at this time.";
        }
    }

    private async Task CreateChatHistoryAsync(long telegramId, string message, string? aiResponse, decimal? responseTime)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var chatHistory = new BboChathistory
            {
                Userid = (int)telegramId,
                Message = message,
                Response = aiResponse,
                Sentat = DateTime.Now,
                LanguageCode = "en",
                Responsetime = responseTime
            };

            var success = await unitOfWork.chatHistoryReponsitory.AddEntity(chatHistory);
            if (success)
            {
                await unitOfWork.CompleteAsync();
                _logger.LogInformation("Chat history created for Telegram ID: {TelegramId}, Chat ID: {ChatId}, Response Time: {ResponseTime}",
                    telegramId, chatHistory.Chatid, responseTime);
            }
            else
            {
                _logger.LogError("Failed to create chat history for Telegram ID: {TelegramId}", telegramId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat history for Telegram ID: {TelegramId}", telegramId);
        }
    }
    #endregion

    #region StopAsync
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }
    #endregion
}