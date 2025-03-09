//using Telegram.Bot;
//using Telegram.Bot.Polling;
//using Telegram.Bot.Types;
//using Telegram.Bot.Types.Enums;
//using Telegram.Bot.Types.ReplyMarkups;
//using ChatBot.API.Interface;
//using ChatBot.API.Models;
//using Microsoft.Extensions.DependencyInjection;
//using System.Text.Json;
//using System.Collections.Generic;
//using System.Text;

//namespace ChatBot.API.Handle;

//public class MyBot : IHostedService
//{
//    #region Khai báo contructor
//    private readonly TelegramBotClient _botClient;
//    private readonly ILogger<MyBot> _logger;
//    private readonly CancellationTokenSource _cancellationTokenSource;
//    private readonly IServiceScopeFactory _serviceScopeFactory;
//    private readonly Dictionary<long, (string State, string Comment)> _feedbackState;
//    private readonly Dictionary<long, int> _filterPageState;

//    public MyBot(ILogger<MyBot> logger, IServiceScopeFactory serviceScopeFactory)
//    {
//        _botClient = new TelegramBotClient("7734778997:AAE0KtrSjeipZAX6-pM_yUxq6rP6N8z2lcs");
//        _logger = logger;
//        _cancellationTokenSource = new CancellationTokenSource();
//        _serviceScopeFactory = serviceScopeFactory;
//        _feedbackState = new Dictionary<long, (string, string)>();
//        _filterPageState = new Dictionary<long, int>();
//    }
//    #endregion

//    #region Start bot & Allow Callback
//    public async Task StartAsync(CancellationToken cancellationToken)
//    {
//        // Đăng ký danh sách lệnh
//        var commands = new[]
//        {
//        new BotCommand { Command = "h", Description = "Show available commands" },
//        new BotCommand { Command = "s", Description = "Account settings" },
//        new BotCommand { Command = "find", Description = "Recommended questions" },
//        new BotCommand { Command = "f", Description = "Send feedback" },
//        new BotCommand { Command = "p", Description = "View achievements" }
//    };

//        await _botClient.SetMyCommandsAsync(commands, cancellationToken: cancellationToken);

//        var receiverOptions = new ReceiverOptions
//        {
//            AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery },
//            ThrowPendingUpdates = true,
//            Limit = 100 // Tăng giới hạn số lượng bản cập nhật xử lý đồng thời
//        };

//        _botClient.StartReceiving(
//            updateHandler: async (botClient, update, ct) => await HandleUpdateAsync(botClient, update, ct),
//            pollingErrorHandler: HandlePollingErrorAsync,
//            receiverOptions: receiverOptions,
//            cancellationToken: _cancellationTokenSource.Token
//        );

//        var me = await _botClient.GetMeAsync(cancellationToken);
//        _logger.LogInformation("Bot started: @{BotUsername}", me.Username);
//    }
//    #endregion

//    #region Xử lý Logic bot
//    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
//    {
//        try
//        {
//            if (update.Message != null)
//            {
//                var chatId = update.Message.Chat.Id;

//                // Xử lý state
//                if (_feedbackState.ContainsKey(chatId) && update.Message.Text != null)
//                {
//                    var (state, _) = _feedbackState[chatId];
//                    if (state == "awaiting_comment")
//                    {
//                        lock (_feedbackState) // Đảm bảo an toàn khi truy cập dictionary
//                        {
//                            _feedbackState[chatId] = ("awaiting_rating", update.Message.Text);
//                        }
//                        await SendRatingButtons(botClient, chatId, cancellationToken);
//                        return;
//                    }
//                    else if (state == "awaiting_onchainid")
//                    {
//                        await UpdateOnchainId(botClient, chatId, update.Message.Text);
//                        return;
//                    }
//                }

//                if (update.Message.Text != null)
//                {
//                    var messageText = update.Message.Text;
//                    _logger.LogInformation("Received message: {Message}", messageText);

//                    if (messageText == "/start")
//                    {
//                        await Task.Run(async () => // Chạy tạo người dùng trên thread riêng
//                        {
//                            using var scope = _serviceScopeFactory.CreateScope();
//                            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//                            var existingUser = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId);

//                            if (existingUser == null)
//                            {
//                                var newUser = new BboUser
//                                {
//                                    Telegramid = (int)chatId,
//                                    Username = update.Message.From?.Username,
//                                    Joindate = DateTime.Now,
//                                    Lastactive = DateTime.Now,
//                                    Isactive = true,
//                                    Roleid = 3 // Default role is User
//                                };

//                                await unitOfWork.userReponsitory.AddEntity(newUser);
//                                await unitOfWork.CompleteAsync();
//                                _logger.LogInformation("Created new user with Telegram ID: {TelegramId}", chatId);
//                            }
//                        }, cancellationToken);

//                        #region Create inline keyboard
//                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
//                        {
//                        new[] { InlineKeyboardButton.WithCallbackData("👤 Settings", "settings"), InlineKeyboardButton.WithCallbackData("💡 Filter", "filter") },
//                        new[] { InlineKeyboardButton.WithCallbackData("📝 Feedback", "feedback"), InlineKeyboardButton.WithCallbackData("🏆 Point", "point") },
//                    });
//                        #endregion

//                        #region Send welcome message
//                        var welcomeMessage =
//                            $"Hi {update.Message.From?.Username}, Welcome to ADA-BBO Bot!\n\n" +
//                            "📖GovernCardanoBot is an intelligent virtual assistant powered by ChatGPT, designed to answer questions related to the Cardano blockchain and its governance activities.\n\n" +
//                            "🌟 Please select an option:\n\n" +
//                            "👤 - Settings: Account Settings\n" +
//                            "💡 - Filters: Recommended Questions\n" +
//                            "📝 - Feedback: Submit Feedback\n" +
//                            "🏆 - Score: View Achievements\n\n" +
//                           "Or you can use the following commands:\n\n" +
//                            "❓/h - Show available commands\n" +
//                            "👤/s - Account settings\n" +
//                            "💡/find - Recommended questions\n" +
//                            "📝/f - Send feedback\n" +
//                            "🏆/p - View achievements\n\n" +
//                            "You can join our community group at: [Cardano_ECO_VN](https://t.me/Cardano_ECO_VN)";

//                        await botClient.SendTextMessageAsync(chatId, welcomeMessage, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken, parseMode: ParseMode.Markdown);
//                        #endregion
//                        return;
//                    }

//                    string responseMessage = await Task.Run(async () => messageText switch
//                    {
//                        "/h" => await GetHelpMessage(botClient, chatId, cancellationToken),
//                        "/s" => await GetSettingsMessage(botClient, chatId, cancellationToken),
//                        "/find" => await HandleFilterCommand(botClient, chatId, cancellationToken).ContinueWith(_ => string.Empty),
//                        "/f" => await StartFeedbackProcess(botClient, chatId, cancellationToken),
//                        "/p" => "🏆 Your Achievement Points",
//                        _ => await GetResponseFromAI(chatId, messageText)
//                    }, cancellationToken);

//                    if (messageText != "/h" && messageText != "/f" && messageText != "/s")
//                    {
//                        await botClient.SendTextMessageAsync(chatId, responseMessage, cancellationToken: cancellationToken);
//                    }
//                }
//            }
//            else if (update.CallbackQuery != null)
//            {
//                await HandleCallbackQuery(botClient, update.CallbackQuery);
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error handling update");
//        }
//    }
//    #endregion

//    #region Settings Process
//    private async Task<string> GetSettingsMessage(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
//    {
//        try
//        {
//            using var scope = _serviceScopeFactory.CreateScope();
//            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//            var user = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId);

//            if (user == null)
//            {
//                return "User not found. Please use /start to register.";
//            }

//            var role = await unitOfWork.roleReponsitory.GetAsync(user.Roleid ?? 0);
//            var roleName = role?.Rolename ?? "User";

//            var settingsMessage = "👤Account Information:\n\n" +
//                                 $" - Username: {user.Username ?? "Not set"}\n" +
//                                 $" - Telegram code: {user.Telegramid}\n" +
//                                 $" - Join date: {user.Joindate?.ToString("dd/MM/yyyy") ?? "N/A"}\n" +
//                                 $" - Status: {(user.Isactive == true ? "Active" : "Inactive")}\n" +
//                                 $" - Role: {roleName}\n" +
//                                 $" - Onchain ID: {user.Onchainid ?? "Not set"}\n\n"+
//                                 "You can update your Onchain Id and participation role by selecting the edit buttons below.\n";

//            var inlineKeyboard = new InlineKeyboardMarkup(new[]
//            {
//                new[] { InlineKeyboardButton.WithCallbackData("🐙 Onchain ID", "update_onchain"), InlineKeyboardButton.WithCallbackData("🐙Role", "update_role") }
//            });

//            await botClient.SendTextMessageAsync(
//                chatId: chatId,
//                text: settingsMessage,
//                replyMarkup: inlineKeyboard,
//                cancellationToken: cancellationToken
//            );

//            return string.Empty;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error fetching settings for Telegram ID: {TelegramId}", chatId);
//            return "Error retrieving your account information.";
//        }
//    }

//    private async Task<string> StartUpdateOnchainProcess(ITelegramBotClient botClient, long chatId)
//    {
//        _feedbackState[chatId] = ("awaiting_onchainid", string.Empty);
//        await botClient.SendTextMessageAsync(
//            chatId: chatId,
//            text: "💻 Please enter your new Onchain ID:",
//            cancellationToken: _cancellationTokenSource.Token
//        );
//        return string.Empty;
//    }

//    private async Task UpdateOnchainId(ITelegramBotClient botClient, long chatId, string newOnchainId)
//    {
//        try
//        {
//            using var scope = _serviceScopeFactory.CreateScope();
//            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//            var user = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId);

//            if (user != null)
//            {
//                user.Onchainid = newOnchainId;
//                await unitOfWork.userReponsitory.UpdateEntity(user);
//                await unitOfWork.CompleteAsync();

//                _feedbackState.Remove(chatId);
//                await botClient.SendTextMessageAsync(
//                    chatId: chatId,
//                    text: "🐳 Onchain ID updated successfully!\n 🐳Use /s to view your updated information.",
//                    cancellationToken: _cancellationTokenSource.Token
//                );
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error updating Onchain ID for Telegram ID: {TelegramId}", chatId);
//            await botClient.SendTextMessageAsync(
//                chatId: chatId,
//                text: "Error updating Onchain ID. Please try again.",
//                cancellationToken: _cancellationTokenSource.Token
//            );
//        }
//    }

//    private async Task<string> StartUpdateRoleProcess(ITelegramBotClient botClient, long chatId)
//    {
//        var roleKeyboard = new InlineKeyboardMarkup(new[]
//        {
//            new[] { InlineKeyboardButton.WithCallbackData("🧑‍🏫Drep", "role_2"), InlineKeyboardButton.WithCallbackData("🧑‍💼User", "role_3") },
//            new[] { InlineKeyboardButton.WithCallbackData("👑SPO", "role_4"), InlineKeyboardButton.WithCallbackData("💰Holder", "role_5") },
//            new[] { InlineKeyboardButton.WithCallbackData("🧑‍⚖️Committee", "role_6") }
//        });

//        await botClient.SendTextMessageAsync(
//            chatId: chatId,
//            text: "💻 Please select your new role:",
//            replyMarkup: roleKeyboard,
//            cancellationToken: _cancellationTokenSource.Token
//        );

//        return string.Empty;
//    }

//    private async Task<string> UpdateRole(ITelegramBotClient botClient, long chatId, int? roleId)
//    {
//        try
//        {
//            using var scope = _serviceScopeFactory.CreateScope();
//            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//            var user = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId);

//            if (user != null)
//            {
//                user.Roleid = roleId; // Nullable int? as per BboUser model
//                await unitOfWork.userReponsitory.UpdateEntity(user);
//                await unitOfWork.CompleteAsync();

//                await botClient.SendTextMessageAsync(
//                    chatId: chatId,
//                    text: "🐳 Role updated successfully!\n🐳Use /s to view your updated information.",
//                    cancellationToken: _cancellationTokenSource.Token
//                );
//            }
//            return string.Empty;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error updating role for Telegram ID: {TelegramId}", chatId);
//            await botClient.SendTextMessageAsync(
//                chatId: chatId,
//                text: "Error updating role. Please try again.",
//                cancellationToken: _cancellationTokenSource.Token
//            );
//            return "Error";
//        }
//    }
//    #endregion

//    #region Feedback Process
//    private async Task<string> StartFeedbackProcess(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
//    {
//        _feedbackState[chatId] = ("awaiting_comment", string.Empty);
//        await botClient.SendTextMessageAsync(chatId, "💻 Please enter your feedback:", cancellationToken: cancellationToken);
//        return string.Empty;
//    }

//    private async Task SendRatingButtons(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
//    {
//        var ratingKeyboard = new InlineKeyboardMarkup(new[]
//        {
//            new[] { InlineKeyboardButton.WithCallbackData("1️⃣", "rate_1"), InlineKeyboardButton.WithCallbackData("2️⃣", "rate_2"), InlineKeyboardButton.WithCallbackData("3️⃣", "rate_3") },
//            new[] { InlineKeyboardButton.WithCallbackData("4️⃣", "rate_4"), InlineKeyboardButton.WithCallbackData("5️⃣", "rate_5"), InlineKeyboardButton.WithCallbackData("⏭️", "rate_6") }
//        });

//        var messageText = "\n🌟Please select your satisfaction level:\n\n" +
//                         "⛈️ Option 1 ➡️ Dissatisfied\n" +
//                         "🌧️ Option 2 ➡️ Slightly disappointed\n" +
//                         "🌱 Option 3 ➡️ Average\n" +
//                         "🔥 Option 4 ➡️ Satisfied\n" +
//                         "🌈 Option 5 ➡️ Very satisfied\n" +
//                         "🌊 Option 6 ➡️ Skip\n\n"+
//                         "Please select one of the options below to let us know your opinion about your chatbot experience!";

//        await botClient.SendTextMessageAsync(
//            chatId: chatId,
//            text: messageText,
//            replyMarkup: ratingKeyboard,
//            cancellationToken: cancellationToken
//        );
//    }

//    private async Task HandleFeedbackRating(ITelegramBotClient botClient, CallbackQuery callbackQuery)
//    {
//        var chatId = callbackQuery.Message.Chat.Id;
//        var callbackData = callbackQuery.Data;

//        if (_feedbackState.ContainsKey(chatId) && _feedbackState[chatId].State == "awaiting_rating" && callbackData.StartsWith("rate_"))
//        {
//            int rating = int.Parse(callbackData.Split('_')[1]);
//            string comment = _feedbackState[chatId].Comment;

//            await SaveFeedback(chatId, rating, comment);
//            _feedbackState.Remove(chatId);

//            await botClient.SendTextMessageAsync(chatId, "Thank you for your feedback! 💖");
//            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
//        }
//    }

//    private async Task SaveFeedback(long telegramId, int rating, string comment)
//    {
//        try
//        {
//            using var scope = _serviceScopeFactory.CreateScope();
//            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

//            var feedback = new BboFeedback
//            {
//                Userid = (int)telegramId,
//                Rating = rating,
//                Comment = comment,
//                Createdat = DateTime.Now
//            };

//            await unitOfWork.feedbackReponsitory.AddEntity(feedback);
//            await unitOfWork.CompleteAsync();
//            _logger.LogInformation("Feedback saved for Telegram ID: {TelegramId}, Rating: {Rating}", telegramId, rating);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error saving feedback for Telegram ID: {TelegramId}", telegramId);
//        }
//    }
//    #endregion

//    #region Handle Filter Command
//    private async Task HandleFilterCommand(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken, int page = 1)
//    {
//        await Task.Run(async () =>
//        {
//            try
//            {
//                using var scope = _serviceScopeFactory.CreateScope();
//                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//                var questions = await unitOfWork.filterReponsitory.GetAllAsync();

//                if (questions == null || !questions.Any())
//                {
//                    await botClient.SendTextMessageAsync(chatId, "No suggested questions available.", cancellationToken: cancellationToken);
//                    return;
//                }

//                const int itemsPerPage = 10;
//                int totalPages = (int)Math.Ceiling(questions.Count / (double)itemsPerPage);
//                page = Math.Max(1, Math.Min(page, totalPages));
//                lock (_filterPageState) // Đảm bảo an toàn khi truy cập dictionary
//                {
//                    _filterPageState[chatId] = page;
//                }

//                var pageQuestions = questions.Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList();
//                var messageText = new StringBuilder("🌟 **List of suggested questions**\n\n");
//                int startIndex = (page - 1) * itemsPerPage + 1;
//                foreach (var (question, index) in pageQuestions.Select((q, i) => (q, i + startIndex)))
//                {
//                    messageText.AppendLine($"{index}. {question.Question}");
//                }
//                messageText.AppendLine($"\nPage {page}/{totalPages}");

//                var buttons = pageQuestions
//                    .Select((q, i) => InlineKeyboardButton.WithCallbackData($"{startIndex + i}", $"filter_question_{(startIndex + i - 1)}"))
//                    .Chunk(5)
//                    .ToList();

//                var navigationButtons = new List<InlineKeyboardButton>();
//                if (page > 1) navigationButtons.Add(InlineKeyboardButton.WithCallbackData("⬅️ Prev", $"filter_page_{page - 1}"));
//                if (page < totalPages) navigationButtons.Add(InlineKeyboardButton.WithCallbackData("➡️ Next", $"filter_page_{page + 1}"));
//                if (navigationButtons.Any()) buttons.Add(navigationButtons.ToArray());

//                var keyboard = new InlineKeyboardMarkup(buttons);
//                await botClient.SendTextMessageAsync(chatId, messageText.ToString(), parseMode: ParseMode.Markdown, replyMarkup: keyboard, cancellationToken: cancellationToken);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error handling filter command for Telegram ID: {TelegramId}", chatId);
//                await botClient.SendTextMessageAsync(chatId, "Error loading suggested questions.", cancellationToken: cancellationToken);
//            }
//        }, cancellationToken);
//    }

//    private async Task HandleFilterQuestionSelection(ITelegramBotClient botClient, long chatId, int questionIndex, CancellationToken cancellationToken)
//    {
//        try
//        {
//            using var scope = _serviceScopeFactory.CreateScope();
//            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//            var questions = await unitOfWork.filterReponsitory.GetAllAsync();

//            if (questionIndex >= 0 && questionIndex < questions.Count)
//            {
//                var selectedQuestion = questions[questionIndex].Question;
//                var aiResponse = await GetResponseFromAI(chatId, selectedQuestion);
//                await botClient.SendTextMessageAsync(chatId, aiResponse, cancellationToken: cancellationToken);
//            }
//            else
//            {
//                await botClient.SendTextMessageAsync(chatId, "Invalid question selected.", cancellationToken: cancellationToken);
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error handling filter question selection for Telegram ID: {TelegramId}", chatId);
//            await botClient.SendTextMessageAsync(chatId, "Error processing your question.", cancellationToken: cancellationToken);
//        }
//    }
//    #endregion

//    #region Gọi lệnh Help Message
//    private async Task<string> GetHelpMessage(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
//    {
//        var helpMessage =
//                    "🌟 Please select an option:\n\n" +
//                    "👤 - Settings: Account Settings\n" +
//                    "💡 - Filters: Recommended Questions\n" +
//                    "📝 - Feedback: Submit Feedback\n" +
//                    "🏆 - Score: View Achievements\n\n" +
//                    "Or you can use the following commands:\n\n" +
//                    "❓/h - Show available commands\n" +
//                    "👤/s - Account settings\n" +
//                    "💡/find - Recommended questions\n" +
//                    "📝/f - Send feedback\n" +
//                    "🏆/p - View achievements\n\n" +
//                    "You can join our community group at: [Cardano_ECO_VN](https://t.me/Cardano_ECO_VN)";

//        var inlineKeyboard = new InlineKeyboardMarkup(new[]
//        {
//            new[] { InlineKeyboardButton.WithCallbackData("👤 Settings", "settings"), InlineKeyboardButton.WithCallbackData("💡 Filter", "filter") },
//            new[] { InlineKeyboardButton.WithCallbackData("📝 Feedback", "feedback"), InlineKeyboardButton.WithCallbackData("🏆 Point", "point") },
//        });

//        await botClient.SendTextMessageAsync(chatId, helpMessage, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
//        return helpMessage;
//    }
//    #endregion

//    #region Add callback query handler
//    private async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
//    {
//        var chatId = callbackQuery.Message.Chat.Id;
//        var callbackData = callbackQuery.Data;

//        var response = callbackData switch
//        {
//            "settings" => await GetSettingsMessage(botClient, chatId, _cancellationTokenSource.Token),
//            "filter" => await HandleFilterCommand(botClient, chatId, _cancellationTokenSource.Token).ContinueWith(_ => string.Empty),
//            "feedback" => await StartFeedbackProcess(botClient, chatId, _cancellationTokenSource.Token),
//            "point" => "🎯 Your Achievement Points",
//            "update_onchain" => await StartUpdateOnchainProcess(botClient, chatId),
//            "update_role" => await StartUpdateRoleProcess(botClient, chatId),
//            var data when data.StartsWith("role_") =>
//                await UpdateRole(botClient, chatId, int.Parse(data.Split('_')[1])).ContinueWith(_ => string.Empty),
//            var data when data.StartsWith("rate_") =>
//                await HandleFeedbackRating(botClient, callbackQuery).ContinueWith(_ => string.Empty),
//            var data when data.StartsWith("filter_page_") =>
//                await HandleFilterCommand(botClient, chatId, _cancellationTokenSource.Token, int.Parse(data.Split('_')[2])).ContinueWith(_ => string.Empty),
//            var data when data.StartsWith("filter_question_") =>
//                await HandleFilterQuestionSelection(botClient, chatId, int.Parse(data.Split('_')[2]), _cancellationTokenSource.Token).ContinueWith(_ => string.Empty),
//            _ => "Invalid option"
//        };

//        if (callbackData != "feedback" &&
//            !callbackData.StartsWith("rate_") &&
//            callbackData != "settings" &&
//            callbackData != "update_onchain" &&
//            callbackData != "update_role" &&
//            !callbackData.StartsWith("role_") &&
//            callbackData != "filter" &&
//            !callbackData.StartsWith("filter_page_") &&
//            !callbackData.StartsWith("filter_question_"))
//        {
//            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
//            await botClient.SendTextMessageAsync(chatId, response);
//        }
//        else
//        {
//            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
//        }
//    }
//    #endregion

//    #region HandlePollingErrorAsync
//    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
//    {
//        _logger.LogError(exception, "Polling error occurred");
//        return Task.CompletedTask;
//    }
//    #endregion

//    #region Gọi API AI Model
//    private async Task<string> GetResponseFromAI(long chatId, string message)
//    {
//        var loadingMessage = await _botClient.SendTextMessageAsync(chatId, "...", cancellationToken: _cancellationTokenSource.Token);
//        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // Timeout 30 giây
//var loadingTask = Task.Run(async () =>
//{
//    string[] dots = new[] { "Typing", "Typing.", "Typing..", "Typing..." };
//    int index = 0;
//    while (!cts.Token.IsCancellationRequested)
//    {
//        await _botClient.EditMessageTextAsync(chatId, loadingMessage.MessageId, dots[index], cancellationToken: cts.Token);
//        index = (index + 1) % dots.Length;
//        await Task.Delay(500, cts.Token);
//    }
//}, cts.Token);

//        try
//        {
//            return await Task.Run(async () =>
//            {
//                using var httpClient = new HttpClient();
//                string apiUrl = "http://aitreviet.duckdns.org:8000";
//                var payload = new { ID = chatId, message = message };
//                var jsonContent = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

//                var response = await httpClient.PostAsync(apiUrl, jsonContent, cts.Token);
//                string aiResponse;
//                decimal? responseTime = null;

//                if (response.IsSuccessStatusCode)
//                {
//                    string jsonResponse = await response.Content.ReadAsStringAsync(cts.Token);
//                    using var doc = JsonDocument.Parse(jsonResponse);
//                    var root = doc.RootElement;
//                    aiResponse = root.GetProperty("answer").GetString() ?? "Sorry, I couldn't process your request.";
//                    responseTime = root.TryGetProperty("time", out var timeElement) ? timeElement.GetDecimal() : null;
//                }
//                else
//                {
//                    _logger.LogError("API call failed with status code: {StatusCode}", response.StatusCode);
//                    aiResponse = "Sorry, I couldn't process your request at this time.";
//                }

//                return aiResponse;
//            }, cts.Token);
//        }
//        catch (OperationCanceledException)
//        {
//            _logger.LogWarning("Request timed out for Telegram ID: {TelegramId}", chatId);
//            cts.Cancel();
//            try { await loadingTask; } catch (TaskCanceledException) { }
//            await _botClient.DeleteMessageAsync(chatId, loadingMessage.MessageId, cts.Token);
//            return "Request timed out. Please try again later.";
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error in GetResponseFromAI for Telegram ID: {TelegramId}", chatId);
//            cts.Cancel();
//            try { await loadingTask; } catch (TaskCanceledException) { }
//            await _botClient.DeleteMessageAsync(chatId, loadingMessage.MessageId, cts.Token);
//            await CreateChatHistoryAsync(chatId, message, null, null);
//            return "Sorry, I couldn't process your request at this time.";
//        }
//        finally
//        {
//            cts.Cancel();
//            try { await loadingTask; } catch (TaskCanceledException) { }
//            await _botClient.DeleteMessageAsync(chatId, loadingMessage.MessageId, _cancellationTokenSource.Token);
//        }
//    }

//    private async Task CreateChatHistoryAsync(long telegramId, string message, string? aiResponse, decimal? responseTime)
//    {
//        try
//        {
//            using var scope = _serviceScopeFactory.CreateScope();
//            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//            var chatHistory = new BboChathistory
//            {
//                Userid = (int)telegramId,
//                Message = message,
//                Response = aiResponse,
//                Sentat = DateTime.Now,
//                LanguageCode = "en",
//                Responsetime = responseTime
//            };

//            var success = await unitOfWork.chatHistoryReponsitory.AddEntity(chatHistory);
//            if (success)
//            {
//                await unitOfWork.CompleteAsync();
//                _logger.LogInformation("Chat history created for Telegram ID: {TelegramId}, Chat ID: {ChatId}, Response Time: {ResponseTime}",
//                    telegramId, chatHistory.Chatid, responseTime);
//            }
//            else
//            {
//                _logger.LogError("Failed to create chat history for Telegram ID: {TelegramId}", telegramId);
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error creating chat history for Telegram ID: {TelegramId}", telegramId);
//        }
//    }
//    #endregion

//    #region StopAsync
//    public Task StopAsync(CancellationToken cancellationToken)
//    {
//        _cancellationTokenSource.Cancel();
//        return Task.CompletedTask;
//    }
//    #endregion
//}