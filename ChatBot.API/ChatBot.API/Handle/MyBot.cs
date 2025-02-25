using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ChatBot.API.Handle;

public class MyBot : IHostedService
{
    #region Khai báo contructor
    private readonly TelegramBotClient _botClient;
    private readonly ILogger<MyBot> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MyBot(ILogger<MyBot> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _botClient = new TelegramBotClient("7445606959:AAGXH77l7KL4d8bpO-E_HHO7LQn5Nfn5HzM");
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
        _serviceScopeFactory = serviceScopeFactory;
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
            updateHandler: async (botClient, update, ct) =>
            {
                if (update.Message != null)
                {
                    await HandleUpdateAsync(botClient, update, ct);
                }
                else if (update.CallbackQuery != null)
                {
                    await HandleCallbackQuery(botClient, update.CallbackQuery);
                }
            },
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
            if (update.Message?.Text == null)
                return;

            var chatId = update.Message.Chat.Id;
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
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("👤 Settings", "settings"),
                        InlineKeyboardButton.WithCallbackData("💡 Filter", "filter"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("📝 Feedback", "feedback"),
                        InlineKeyboardButton.WithCallbackData("🎯 Point", "point"),
                    },
                });
                #endregion


                #region Send welcome message
                var welcomeMessage = 
                    $"Hi {update.Message.From.Username}, Welcome to ADA-BBO Bot!\n\n" +
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

                // Send message with inline keyboard
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: welcomeMessage,
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken
                );
                #endregion

                return;
            }

            // Handle other commands
            string responseMessage = messageText switch
            {
                "/help" => await GetHelpMessage(botClient, chatId, cancellationToken),
                "/settings" => "⚙️ Account Settings Menu",
                "/filter" => "🔍 Filter Options Menu",
                "/feedback" => "📝 Feedback Form",
                "/point" => "🎯 Your Achievement Points",
                _ => await GetResponseFromAI(chatId, messageText)
            };

            if (messageText != "/help") // Only send message if it's not /help (since /help sends its own message)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: responseMessage,
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling update");
        }
    }
    #endregion

    #region Gọi lện Help Message
    private async Task<string> GetHelpMessage(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var helpMessage = "Available commands:\n" +
                         "────────────────────────────────\n" +
                         "💡 - /help - Show available commands\n" +
                         "👤 - /settings - Account settings\n" +
                         "💡 - /filter - Suggested questions\n" +
                         "📝 - /feedback - Submit feedback\n" +
                         "🎯 - /point - View achievements";

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData("👤 Settings", "settings"),
                InlineKeyboardButton.WithCallbackData("💡 Filter", "filter"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData("📝 Feedback", "feedback"),
                InlineKeyboardButton.WithCallbackData("🎯 Point", "point"),
            },
        });

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: helpMessage,
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken
        );

        return helpMessage;
    }
    #endregion

    #region Add callback query handler
    private async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var response = callbackQuery.Data switch
        {
            "settings" => "⚙️ Account Settings Menu",
            "filter" => "🔍 Filter Options Menu",
            "feedback" => "📝 Feedback Form",
            "point" => "🎯 Your Achievement Points",
            _ => "Invalid option"
        };

        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, response);
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
        try
        {
            // First, create chat history entry with the user's message
            int? chatHistoryId = await CreateChatHistoryAsync(chatId, message);
            if (chatHistoryId == null)
            {
                _logger.LogError("Failed to create chat history for user {TelegramId}", chatId);
                return "Sorry, I couldn't process your request at this time.";
            }

            // Then make the API call
            using var httpClient = new HttpClient();
            string apiUrl = "http://aitreviet.duckdns.org:8000";

            var payload = new
            {
                ID = chatId,
                message = message
            };

            var jsonContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await httpClient.PostAsync(apiUrl, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(jsonResponse);
                var aiResponse = doc.RootElement.GetProperty("answer").GetString() ??
                    "Sorry, I couldn't process your request.";

                // Update the chat history with AI response
                await UpdateChatHistoryResponseAsync(chatHistoryId.Value, aiResponse);

                return aiResponse;
            }
            else
            {
                _logger.LogError("API call failed with status code: {StatusCode}", response.StatusCode);
                return "Sorry, I couldn't process your request at this time.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetResponseFromAI");
            return "Sorry, I couldn't process your request at this time.";
        }
    }

    private async Task<int?> CreateChatHistoryAsync(long telegramId, string message)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Get user by telegram ID
            var user = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)telegramId);
            if (user == null)
            {
                _logger.LogWarning("User not found for Telegram ID: {TelegramId}", telegramId);
                return null;
            }

            var chatHistory = new BboChathistory
            {
                Userid = user.Id,
                Message = message,
                Sentat = DateTime.Now,
                LanguageCode = "en", // You can modify this based on actual message language detection
                Updateat = DateTime.Now
            };

            var success = await unitOfWork.chatHistoryReponsitory.AddEntity(chatHistory);
            if (success)
            {
                await unitOfWork.CompleteAsync();
                return chatHistory.Chatid; // Return the ID of the created chat history
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat history");
            return null;
        }
    }

    private async Task UpdateChatHistoryResponseAsync(int chatHistoryId, string aiResponse)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var chatHistory = await unitOfWork.chatHistoryReponsitory.GetAsync(chatHistoryId);
            if (chatHistory != null)
            {
                chatHistory.Response = aiResponse;
                chatHistory.Updateat = DateTime.Now;
                await unitOfWork.CompleteAsync();
            }
            else
            {
                _logger.LogWarning("Chat history not found for ID: {ChatHistoryId}", chatHistoryId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating chat history response");
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
