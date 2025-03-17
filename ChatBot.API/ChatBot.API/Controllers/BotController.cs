using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using System;
using System.Diagnostics;
using Google.Cloud.Translation.V2;
using Microsoft.Extensions.Configuration;
using ChatBot.API.Helpers;
using ChatBot.API.Handle;
using System.Net.Http;
using Polly;
using Microsoft.Extensions.Caching.Memory;
using Polly.Retry;
using Polly.Timeout;

namespace ChatBot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<BotController> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<long, (string State, string Comment)> _feedbackState; // Optimized: Use ConcurrentDictionary for thread safety
        private readonly ConcurrentDictionary<long, int> _filterPageState; // Optimized: Use ConcurrentDictionary for thread safety
        private readonly HashSet<long> _processedUpdateIds;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10, 10); // Optimized: Limit concurrent requests to avoid overloading

        public BotController(ITelegramBotClient botClient, IServiceScopeFactory serviceScopeFactory, ILogger<BotController> logger,
            IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _feedbackState = new ConcurrentDictionary<long, (string, string)>();
            _filterPageState = new ConcurrentDictionary<long, int>();
            _processedUpdateIds = new HashSet<long>();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update, CancellationToken cancellationToken)
        {
            if (update == null) return Ok();

            var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message.Chat.Id ?? 0;
            var updateId = update.Id;

            if (_processedUpdateIds.Contains(updateId))
            {
                _logger.LogWarning("Duplicate update detected with UpdateId: {UpdateId}, skipping processing", updateId);
                return Ok();
            }

            await _semaphore.WaitAsync(cancellationToken); // Optimized: Limit concurrent requests
            try
            {
                if (update.Message != null)
                {
                    var messageText = update.Message.Text;
                    _logger.LogInformation("Received message from chat {ChatId}: {Message} with UpdateId {UpdateId}", chatId, messageText, updateId);

                    if (_feedbackState.ContainsKey(chatId) && messageText != null)
                    {
                        var (state, _) = _feedbackState[chatId];
                        if (state == "awaiting_onchainid")
                        {
                            await UpdateOnchainId(chatId, messageText, cancellationToken);
                            return Ok();
                        }
                    }

                    if (messageText != null)
                    {
                        string responseMessage = string.Empty;
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                            switch (messageText.Split(' ')[0])
                            {
                                case "/start":
                                    var existingUser = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId);
                                    string userLanguage = existingUser?.Language ?? "en";

                                    if (existingUser == null)
                                    {
                                        var newUser = new BboUser
                                        {
                                            Telegramid = (int)chatId,
                                            Username = update.Message.From?.Username ?? "User",
                                            Joindate = DateTime.Now,
                                            Lastactive = DateTime.Now,
                                            Isactive = true,
                                            Language = "en",
                                            Roleid = 3
                                        };

                                        await unitOfWork.userReponsitory.AddEntity(newUser);
                                        await unitOfWork.CompleteAsync();
                                        _logger.LogInformation("Created new user with Telegram ID: {TelegramId}", chatId);
                                    }

                                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                    {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData(LanguageResource.GetTranslation(userLanguage, "SettingsButton"), "settings"),
                                            InlineKeyboardButton.WithCallbackData(LanguageResource.GetTranslation(userLanguage, "FilterButton"), "filter")
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData(LanguageResource.GetTranslation(userLanguage, "FeedbackButton"), "feedback"),
                                            InlineKeyboardButton.WithCallbackData(LanguageResource.GetTranslation(userLanguage, "PointButton"), "point")
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData(LanguageResource.GetTranslation(userLanguage, "LanguageButton"), "languages")
                                        }
                                    });

                                    string username = update.Message.From?.Username ?? "User";
                                    var welcomeMessage = LanguageResource.GetTranslation(userLanguage, "WelcomeMessage", username);

                                    await _botClient.SendTextMessageAsync(chatId, welcomeMessage, replyMarkup: inlineKeyboard,
                                        cancellationToken: cancellationToken, parseMode: ParseMode.Markdown);
                                    break;

                                case "/h":
                                    var helpMessage = await GetHelpMessage(chatId, cancellationToken);
                                    var helpKeyboard = await GetHelpInlineKeyboard(chatId, cancellationToken);
                                    await _botClient.SendTextMessageAsync(chatId, helpMessage, replyMarkup: helpKeyboard,
                                        cancellationToken: cancellationToken, parseMode: ParseMode.Markdown);
                                    break;

                                case "/s":
                                    responseMessage = await GetSettingsMessage(chatId, cancellationToken);
                                    break;

                                case "/find":
                                    responseMessage = await HandleFilterCommand(chatId, cancellationToken);
                                    break;

                                case "/p":
                                    responseMessage = "🏆 Your Achievement Points";
                                    break;

                                case "/f":
                                    string feedback = string.Join(" ", messageText.Split(' ').Skip(1));
                                    userLanguage = (await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId))?.Language ?? "en";
                                    if (string.IsNullOrWhiteSpace(feedback))
                                    {
                                        responseMessage = LanguageResource.GetTranslation(userLanguage, "FeedbackPrompt");
                                    }
                                    else
                                    {
                                        await SaveFeedback(chatId, 0, feedback, cancellationToken);
                                        responseMessage = LanguageResource.GetTranslation(userLanguage, "FeedbackThanks");
                                    }
                                    break;

                                case "/la":
                                    await ShowLanguageOptions(chatId, cancellationToken);
                                    responseMessage = string.Empty;
                                    break;

                                default:
                                    responseMessage = await GetResponseFromAI(chatId, messageText, cancellationToken);
                                    break;
                            }
                        }

                        if (!string.IsNullOrEmpty(responseMessage))
                        {
                            _logger.LogInformation("Sending response: {ResponseMessage} to chat {ChatId} for UpdateId {UpdateId}",
                                responseMessage, chatId, updateId);
                            await _botClient.SendTextMessageAsync(chatId, responseMessage, cancellationToken: cancellationToken);
                        }
                    }
                }
                else if (update.CallbackQuery != null)
                {
                    await HandleCallbackQuery(update.CallbackQuery, cancellationToken);
                }

                _processedUpdateIds.Add(updateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling webhook update for chat {ChatId} with UpdateId {UpdateId}", chatId, updateId);
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                _semaphore.Release(); // Optimized: Release semaphore after processing
            }

            return Ok();
        }

        #region Language Process
        private async Task ShowLanguageOptions(long chatId, CancellationToken cancellationToken)
        {
            var (user, userLanguage) = await GetUserLanguageAsync(chatId, cancellationToken);

            var languageKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🇻🇳 VI", "lang_vi"),
                    InlineKeyboardButton.WithCallbackData("🇬🇧 EN", "lang_en")
                }
            });

            await _botClient.SendTextMessageAsync(
                chatId,
                LanguageResource.GetTranslation(userLanguage, "LanguagePrompt"),
                replyMarkup: languageKeyboard,
                cancellationToken: cancellationToken
            );
        }

        private async Task<string> UpdateUserLanguage(long chatId, string language, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var user = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId);

                if (user == null)
                {
                    return LanguageResource.GetTranslation("en", "NoUser");
                }

                user.Language = language;
                await unitOfWork.userReponsitory.UpdateEntity(user);
                await unitOfWork.CompleteAsync();

                // Optimized: Invalidate cache after language update
                _cache.Remove($"user:{chatId}");

                var message = LanguageResource.GetTranslation(language, "LanguageUpdated");
                await _botClient.SendTextMessageAsync(chatId, message, cancellationToken: cancellationToken);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating language for Telegram ID: {TelegramId}", chatId);
                return language == "vi"
                    ? "Lỗi khi cập nhật ngôn ngữ. Vui lòng thử lại."
                    : "Error updating language. Please try again.";
            }
        }
        #endregion

        #region Settings Process
        private async Task<string> GetSettingsMessage(long chatId, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var user = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId);

                if (user == null)
                {
                    return LanguageResource.GetTranslation("en", "NoUser");
                }

                var role = await unitOfWork.roleReponsitory.GetAsync(user.Roleid ?? 0);
                var roleName = role?.Rolename ?? "User";
                var userLanguage = user.Language ?? "en";

                string languageDisplayName = userLanguage switch
                {
                    "en" => "English",
                    "vi" => "Vietnamese",
                    _ => userLanguage
                };

                var settingsMessage = LanguageResource.GetTranslation(userLanguage, "SettingsMessage",
                    user.Username ?? "Not set",
                    user.Telegramid,
                    user.Joindate?.ToString("dd/MM/yyyy") ?? "N/A",
                    user.Isactive == true ? "Active" : "Inactive",
                    roleName,
                    user.Onchainid ?? "Not set",
                    languageDisplayName);

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(LanguageResource.GetTranslation(userLanguage, "OnchainIdButton"), "update_onchain"),
                        InlineKeyboardButton.WithCallbackData(LanguageResource.GetTranslation(userLanguage, "RoleButton"), "update_role")
                    }
                });

                await _botClient.SendTextMessageAsync(chatId, settingsMessage, replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken, parseMode: ParseMode.Markdown);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching settings for Telegram ID: {TelegramId}", chatId);
                return LanguageResource.GetTranslation("en", "AIError");
            }
        }

        private async Task<string> StartUpdateOnchainProcess(long chatId, CancellationToken cancellationToken)
        {
            var (user, userLanguage) = await GetUserLanguageAsync(chatId, cancellationToken);
            _feedbackState[chatId] = ("awaiting_onchainid", string.Empty); // Optimized: No lock needed with ConcurrentDictionary
            await _botClient.SendTextMessageAsync(chatId, LanguageResource.GetTranslation(userLanguage, "OnchainIdPrompt"),
                cancellationToken: cancellationToken);
            return string.Empty;
        }

        private async Task<string> UpdateOnchainId(long chatId, string newOnchainId, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var user = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId);
                var userLanguage = user?.Language ?? "en";

                if (user != null)
                {
                    user.Onchainid = newOnchainId;
                    await unitOfWork.userReponsitory.UpdateEntity(user);
                    await unitOfWork.CompleteAsync();

                    _feedbackState.TryRemove(chatId, out _); // Optimized: No lock needed with ConcurrentDictionary
                    await _botClient.SendTextMessageAsync(chatId, LanguageResource.GetTranslation(userLanguage, "OnchainIdSuccess"),
                        cancellationToken: cancellationToken);
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Onchain ID for Telegram ID: {TelegramId}", chatId);
                await _botClient.SendTextMessageAsync(chatId, "Error updating Onchain ID. Please try again.",
                    cancellationToken: cancellationToken);
                return "Error";
            }
        }

        private async Task<string> StartUpdateRoleProcess(long chatId, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var user = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId);
            var userLanguage = user?.Language ?? "en";
            var roleKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("🧑‍🏫Drep", "role_2"), InlineKeyboardButton.WithCallbackData("🧑‍💼User", "role_3") },
                new[] { InlineKeyboardButton.WithCallbackData("👑SPO", "role_4"), InlineKeyboardButton.WithCallbackData("💰Holder", "role_5") },
                new[] { InlineKeyboardButton.WithCallbackData("🧑‍⚖️Committee", "role_6") }
            });

            await _botClient.SendTextMessageAsync(chatId, LanguageResource.GetTranslation(userLanguage, "SelectNewRole"),
                replyMarkup: roleKeyboard, cancellationToken: cancellationToken);
            return string.Empty;
        }

        private async Task<string> UpdateRole(long chatId, int? roleId, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var user = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId);
                var userLanguage = user?.Language ?? "en";

                if (user != null)
                {
                    user.Roleid = roleId;
                    await unitOfWork.userReponsitory.UpdateEntity(user);
                    await unitOfWork.CompleteAsync();

                    await _botClient.SendTextMessageAsync(chatId, LanguageResource.GetTranslation(userLanguage, "RoleSuccess"),
                        cancellationToken: cancellationToken, parseMode: ParseMode.Markdown);
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role for Telegram ID: {TelegramId}", chatId);
                await _botClient.SendTextMessageAsync(chatId, "Error updating role. Please try again.",
                    cancellationToken: cancellationToken);
                return "Error";
            }
        }
        #endregion

        #region Feedback Process
        private async Task<string> SaveFeedback(long telegramId, int rating, string comment, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var feedback = new BboFeedback
                {
                    Userid = (int)telegramId,
                    Comment = string.IsNullOrEmpty(comment) ? "No comment provided" : comment,
                    Createdat = DateTime.Now
                };

                await unitOfWork.feedbackReponsitory.AddEntity(feedback);
                await unitOfWork.CompleteAsync();
                _logger.LogInformation("Feedback saved for Telegram ID: {TelegramId}, Comment: {Comment}", telegramId, comment);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving feedback for Telegram ID: {TelegramId}", telegramId);
                return "Error saving feedback";
            }
        }
        #endregion

        #region Handle Filter Command
        private async Task<string> HandleFilterCommand(long chatId, CancellationToken cancellationToken, int page = 1)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var questions = await unitOfWork.filterReponsitory.GetAllAsync();

                if (questions == null || !questions.Any())
                {
                    await _botClient.SendTextMessageAsync(chatId, "No suggested questions available.",
                        cancellationToken: cancellationToken);
                    return string.Empty;
                }

                const int itemsPerPage = 10;
                int totalPages = (int)Math.Ceiling(questions.Count / (double)itemsPerPage);
                page = Math.Max(1, Math.Min(page, totalPages));
                _filterPageState[chatId] = page; // Optimized: No lock needed with ConcurrentDictionary

                var pageQuestions = questions.Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList();
                var messageText = new StringBuilder("🌟 *List of suggested questions*\n\n");
                int startIndex = (page - 1) * itemsPerPage + 1;
                foreach (var (question, index) in pageQuestions.Select((q, i) => (q, i + startIndex)))
                {
                    messageText.AppendLine($"*{index}.* _{question.Question}_");
                }
                messageText.AppendLine($"\nPage {page}/{totalPages}");

                var buttons = pageQuestions
                    .Select((q, i) => InlineKeyboardButton.WithCallbackData($"{startIndex + i}", $"filter_question_{(startIndex + i - 1)}"))
                    .Chunk(5)
                    .ToList();

                var navigationButtons = new List<InlineKeyboardButton>();
                if (page > 1) navigationButtons.Add(InlineKeyboardButton.WithCallbackData("⬅️ Prev", $"filter_page_{page - 1}"));
                if (page < totalPages) navigationButtons.Add(InlineKeyboardButton.WithCallbackData("➡️ Next", $"filter_page_{page + 1}"));
                if (navigationButtons.Any()) buttons.Add(navigationButtons.ToArray());

                var keyboard = new InlineKeyboardMarkup(buttons);
                await _botClient.SendTextMessageAsync(chatId, messageText.ToString(), parseMode: ParseMode.Markdown,
                    replyMarkup: keyboard, cancellationToken: cancellationToken);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling filter command for Telegram ID: {TelegramId}", chatId);
                await _botClient.SendTextMessageAsync(chatId, "Error loading suggested questions.",
                    cancellationToken: cancellationToken);
                return string.Empty;
            }
        }

        private async Task<string> HandleFilterQuestionSelection(long chatId, int questionIndex, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var questions = await unitOfWork.filterReponsitory.GetAllAsync();

                if (questionIndex >= 0 && questionIndex < questions.Count)
                {
                    var selectedQuestion = questions[questionIndex].Question;
                    var aiResponse = await GetResponseFromAI(chatId, selectedQuestion, cancellationToken);
                    await _botClient.SendTextMessageAsync(chatId, aiResponse, cancellationToken: cancellationToken);
                }
                else
                {
                    await _botClient.SendTextMessageAsync(chatId, "Invalid question selected.",
                        cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling filter question selection for Telegram ID: {TelegramId}", chatId);
                await _botClient.SendTextMessageAsync(chatId, "Error processing your question.",
                    cancellationToken: cancellationToken);
            }
            return string.Empty;
        }
        #endregion

        #region Help Message
        private async Task<string> GetHelpMessage(long chatId, CancellationToken cancellationToken)
        {
            var (user, userLanguage) = await GetUserLanguageAsync(chatId, cancellationToken);
            return LanguageResource.GetTranslation(userLanguage, "HelpMessage");
        }

        private async Task<InlineKeyboardMarkup> GetHelpInlineKeyboard(long chatId, CancellationToken cancellationToken)
        {
            var (user, userLanguage) = await GetUserLanguageAsync(chatId, cancellationToken);
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(LanguageResource.GetTranslation(userLanguage, "SettingsButton"), "settings"),
                    InlineKeyboardButton.WithCallbackData(LanguageResource.GetTranslation(userLanguage, "FilterButton"), "filter")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(LanguageResource.GetTranslation(userLanguage, "FeedbackButton"), "feedback"),
                    InlineKeyboardButton.WithCallbackData(LanguageResource.GetTranslation(userLanguage, "PointButton"), "point")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(LanguageResource.GetTranslation(userLanguage, "LanguageButton"), "languages")
                }
            });
        }
        #endregion

        #region Callback Handler
        private async Task HandleCallbackQuery(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            var callbackData = callbackQuery.Data;
            var (user, userLanguage) = await GetUserLanguageAsync(chatId, cancellationToken);

            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Processing...", cancellationToken: cancellationToken);

            var responseTask = callbackData switch
            {
                "settings" => GetSettingsMessage(chatId, cancellationToken),
                "filter" => HandleFilterCommand(chatId, cancellationToken),
                "feedback" => Task.FromResult(LanguageResource.GetTranslation(userLanguage, "FeedbackPrompt")),
                "point" => Task.FromResult(LanguageResource.GetTranslation(userLanguage, "PointMessage")),
                "update_onchain" => StartUpdateOnchainProcess(chatId, cancellationToken),
                "update_role" => StartUpdateRoleProcess(chatId, cancellationToken),
                var data when data.StartsWith("role_") => UpdateRole(chatId, int.Parse(data.Split('_')[1]), cancellationToken),
                var data when data.StartsWith("filter_page_") => HandleFilterCommand(chatId, cancellationToken, int.Parse(data.Split('_')[2])),
                var data when data.StartsWith("filter_question_") => HandleFilterQuestionSelection(chatId, int.Parse(data.Split('_')[2]), cancellationToken),
                "lang_vi" => UpdateUserLanguage(chatId, "vi", cancellationToken),
                "lang_en" => UpdateUserLanguage(chatId, "en", cancellationToken),
                "languages" => ShowLanguageOptions(chatId, cancellationToken).ContinueWith(_ => string.Empty),
                _ => Task.FromResult(LanguageResource.GetTranslation(userLanguage, "InvalidOption"))
            };

            var response = await responseTask;

            if (!string.IsNullOrEmpty(response))
            {
                await _botClient.SendTextMessageAsync(chatId, response, cancellationToken: cancellationToken);
            }
        }
        #endregion

        #region GetUserLanguageAsync
        private async Task<(BboUser? User, string Language)> GetUserLanguageAsync(long chatId, CancellationToken cancellationToken)
        {
            string cacheKey = $"user:{chatId}";
            if (_cache.TryGetValue(cacheKey, out (BboUser?, string) cachedResult))
            {
                return cachedResult; // Optimized: Use cache to reduce database calls
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var user = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId);
            var userLanguage = user?.Language ?? "en";
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10));
            _cache.Set(cacheKey, (user, userLanguage), cacheEntryOptions); // Optimized: Cache user data
            return (user, userLanguage);
        }
        #endregion

        #region AI Response
        private async Task<string> GetResponseFromAI(long chatId, string message, CancellationToken cancellationToken)
        {

            CancellationTokenSource cts = new CancellationTokenSource(); // Để hủy animation khi cần
            Message loadingMessage = null; // Lưu tin nhắn loading để chỉnh sửa
            Task loadingTask = null; // Task chạy animation

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var translateService = scope.ServiceProvider.GetRequiredService<GoogleTranslateService>();
                var httpClient = new HttpClient();

                // 1. Gửi ChatAction và tin nhắn loading ban đầu
                await _botClient.SendChatActionAsync(chatId, ChatAction.Typing, cancellationToken: cancellationToken);
                loadingMessage = await _botClient.SendTextMessageAsync(chatId, "typing...", cancellationToken: cancellationToken);

                // 2. Bắt đầu hiệu ứng animation loading
                loadingTask = Task.Run(async () =>
                {
                    string[] dots = new[] { "typing.", "typing..", "typing..." };
                    int index = 0;
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            await _botClient.EditMessageTextAsync(chatId, loadingMessage.MessageId, dots[index], cancellationToken: cts.Token);
                            index = (index + 1) % dots.Length;
                            await Task.Delay(500, cts.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                    }
                }, cts.Token);


                // 3. Lấy thông tin người dùng và thiết lập ngôn ngữ
                var (user, userLanguage) = await GetUserLanguageAsync(chatId, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning("No user found for chatId {ChatId}", chatId);
                    return "Sorry, I couldn't find your user information.";
                }

                // 4. Lấy lịch sử trò chuyện (5 bản ghi gần nhất)
                var chatHistory = (await unitOfWork.chatHistoryReponsitory.GetAllAsync())
                    .Where(ch => ch.Userid == (int)chatId)
                    .OrderByDescending(ch => ch.Sentat)
                    .Take(5);

                // 5. Chuẩn bị mảng messages cho mô hình (luôn bằng tiếng Anh)
                var messages = new List<object>();
                foreach (var history in chatHistory.OrderBy(ch => ch.Sentat))
                {
                    messages.Add(new { role = "user", content = history.Message });
                    if (!string.IsNullOrEmpty(history.Response))
                        messages.Add(new { role = "assistant", content = history.Response });
                }

                // 6. Xử lý tin nhắn hiện tại dựa trên thiết lập ngôn ngữ
                string currentMessageEn;
                if (userLanguage == "vi")
                {
                    currentMessageEn = await translateService.DetectAndTranslateToEnglishAsync(message);
                }
                else // userLanguage == "en"
                {
                    currentMessageEn = message;
                }
                messages.Add(new { role = "user", content = currentMessageEn });

                // 7. Chuẩn bị JSON payload
                var requestPayload = new
                {
                    model = "bbo_3b_fined_core_v1",
                    messages = messages,
                    temperature = 0.7,
                    max_tokens = -1,
                    stream = false
                };

                var jsonPayload = JsonSerializer.Serialize(requestPayload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // 8. Gửi yêu cầu đến LM Studio
                var stopwatch = Stopwatch.StartNew();
                var response = await httpClient.PostAsync("http://14.241.247.68:8080/v1/chat/completions", content, cancellationToken);
                stopwatch.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get response from LM Studio. Status: {StatusCode}", response.StatusCode);
                    return "Sorry, I couldn't process your request right now.";
                }

                // 9. Xử lý phản hồi từ LM Studio
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<JsonElement>(responseJson);
                var aiResponseEn = responseData.GetProperty("choices")[0]
                                              .GetProperty("message")
                                              .GetProperty("content")
                                              .GetString();

                // 10. Chuẩn bị phản hồi cho người dùng dựa trên thiết lập ngôn ngữ
                string finalResponse;
                if (userLanguage == "vi")
                {
                    finalResponse = await translateService.TranslateToVietnameseAsync(aiResponseEn ?? "Thank you");
                }
                else // userLanguage == "en"
                {
                    finalResponse = aiResponseEn ?? "Thank you";
                }

                // 11. Lưu lịch sử
                await CreateChatHistoryAsync(chatId, currentMessageEn, aiResponseEn, (decimal)stopwatch.Elapsed.TotalSeconds, cancellationToken);
                // await CreateChatHistoryAsync(chatId, message, aiResponseEn, (decimal)stopwatch.Elapsed.TotalSeconds, cancellationToken);

                // 12. Dừng animation và xóa tin nhắn loading
                cts.Cancel();
                await Task.WhenAny(loadingTask); // Chờ animation dừng
                await _botClient.DeleteMessageAsync(chatId, loadingMessage.MessageId, cancellationToken: cancellationToken);

                _logger.LogInformation("Received response from LM Studio for chat {ChatId}: {ResponseEn} -> Final response: {FinalResponse}",
                    chatId, aiResponseEn, finalResponse);
                return finalResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling LM Studio for chat {ChatId}", chatId);
                if (loadingMessage != null)
                {
                    cts.Cancel();
                    await Task.WhenAny(loadingTask);
                    await _botClient.DeleteMessageAsync(chatId, loadingMessage.MessageId, cancellationToken: cancellationToken);
                }
                return "Sorry, an error occurred while processing your request.";
            }

        }

        private async Task CreateChatHistoryAsync(long telegramId, string message, string? aiResponse, decimal? responseTime, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var (user, userLanguage) = await GetUserLanguageAsync(telegramId, cancellationToken);
                var chatHistory = new BboChathistory
                {
                    Userid = (int)telegramId,
                    Message = message,
                    Response = aiResponse,
                    Sentat = DateTime.Now,
                    LanguageCode = userLanguage,
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
    }
}