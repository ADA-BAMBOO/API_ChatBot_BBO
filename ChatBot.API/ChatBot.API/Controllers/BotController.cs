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
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using System; // Thêm để sử dụng HashSet

namespace ChatBot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<BotController> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Dictionary<long, (string State, string Comment)> _feedbackState;
        private readonly Dictionary<long, int> _filterPageState;
        private readonly HashSet<long> _processedUpdateIds; // Theo dõi UpdateId đã xử lý
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public BotController(ITelegramBotClient botClient, IServiceScopeFactory serviceScopeFactory, ILogger<BotController> logger)
        {
            _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _feedbackState = new Dictionary<long, (string, string)>();
            _filterPageState = new Dictionary<long, int>();
            _processedUpdateIds = new HashSet<long>(); // Khởi tạo HashSet
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update, CancellationToken cancellationToken)
        {
            if (update == null) return Ok();

            var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message.Chat.Id ?? 0;
            var updateId = update.Id; // Lấy UpdateId từ bản cập nhật

            // Kiểm tra nếu bản cập nhật đã được xử lý
            if (_processedUpdateIds.Contains(updateId))
            {
                _logger.LogWarning("Duplicate update detected with UpdateId: {UpdateId}, skipping processing", updateId);
                return Ok();
            }

            try
            {
                if (update.Message != null)
                {
                    var messageText = update.Message.Text;
                    _logger.LogInformation("Received message from chat {ChatId}: {Message} with UpdateId {UpdateId}", chatId, messageText, updateId);

                    // Xử lý state trước khi vào switch
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

                            switch (messageText.Split(' ')[0]) // Chỉ lấy phần đầu tiên trước dấu cách
                            {
                                case "/start":
                                    var existingUser = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId);

                                    if (existingUser == null)
                                    {
                                        var newUser = new BboUser
                                        {
                                            Telegramid = (int)chatId,
                                            Username = update.Message.From?.Username ?? "User",
                                            Joindate = DateTime.Now,
                                            Lastactive = DateTime.Now,
                                            Isactive = true,
                                            Roleid = 3 // Default role is User
                                        };

                                        await unitOfWork.userReponsitory.AddEntity(newUser);
                                        await unitOfWork.CompleteAsync();
                                        _logger.LogInformation("Created new user with Telegram ID: {TelegramId}", chatId);
                                    }

                                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                    {
                                        new[] { InlineKeyboardButton.WithCallbackData("👤 Settings", "settings"), InlineKeyboardButton.WithCallbackData("💡 Filter", "filter") },
                                        new[] { InlineKeyboardButton.WithCallbackData("📝 Feedback", "feedback"), InlineKeyboardButton.WithCallbackData("🏆 Point", "point") }
                                    });

                                    string username = update.Message.From?.Username ?? "User";
                                    var welcomeMessage =
                                        $"Hi *{username}*, Welcome to *GovernCardanoBot*!\n\n" +
                                        "📖 *GovernCardanoBot* is an intelligent virtual assistant powered by ChatGPT, designed to answer questions related to the Cardano blockchain and its governance activities.\n\n" +
                                        "🌟 *Please select an option:*\n\n" +
                                        "👤 - *Settings*: _Account Settings_\n" +
                                        "💡 - *Filters*: _Recommended Questions_\n" +
                                        "📝 - *Feedback*: _Submit Feedback_\n" +
                                        "🏆 - *Score*: _View Achievements_\n\n" +
                                        "Or you can use the following commands:\n\n" +
                                        "❓ */h* - _Show available commands_\n" +
                                        "👤 */s* - _Account settings_\n" +
                                        "💡 */find* - _Recommended questions_\n" +
                                        "📝 */f* - _Send feedback_\n" +
                                        "🏆 */p* - _View achievements_\n\n" +
                                        "You can join our community group at: *[Cardano_ECO_VN](https://t.me/Cardano_ECO_VN)*";

                                    await _botClient.SendTextMessageAsync(chatId, welcomeMessage, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken, parseMode: ParseMode.Markdown);
                                    break;

                                case "/h":
                                    var helpMessage = await GetHelpMessage(chatId, cancellationToken); // Chỉ lấy nội dung
                                    responseMessage = null; // Không cần gán lại để tránh gửi lần nữa
                                    await _botClient.SendTextMessageAsync(chatId, helpMessage, replyMarkup: GetHelpInlineKeyboard(), cancellationToken: cancellationToken, parseMode: ParseMode.Markdown);
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
                                    string feedback = string.Join(" ", messageText.Split(' ').Skip(1)); // Lấy phần còn lại sau "/f"
                                    if (string.IsNullOrWhiteSpace(feedback))
                                    {
                                        responseMessage = "Please provide feedback with the command. Example: /f This is my feedback";
                                    }
                                    else
                                    {
                                        await SaveFeedback(chatId, 0, feedback, cancellationToken); // Lưu feedback mà không cần rating
                                        responseMessage = "Thank you for your feedback! 💖";
                                    }
                                    break;

                                default:
                                    responseMessage = "Thank you";
                                    break;
                            }
                        }

                        // Đảm bảo gửi phản hồi cho các lệnh khác (trừ /h)
                        if (!string.IsNullOrEmpty(responseMessage))
                        {
                            _logger.LogInformation("Sending response: {ResponseMessage} to chat {ChatId} for UpdateId {UpdateId}", responseMessage, chatId, updateId);
                            await _botClient.SendTextMessageAsync(chatId, responseMessage, cancellationToken: cancellationToken);
                        }
                    }
                }
                else if (update.CallbackQuery != null)
                {
                    await HandleCallbackQuery(update.CallbackQuery, cancellationToken);
                }

                // Đánh dấu bản cập nhật đã xử lý
                _processedUpdateIds.Add(updateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling webhook update for chat {ChatId} with UpdateId {UpdateId}", chatId, update.Id);
                return StatusCode(500, "Internal server error");
            }

            return Ok();
        }

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
                    return "User not found. Please use /start to register.";
                }

                var role = await unitOfWork.roleReponsitory.GetAsync(user.Roleid ?? 0);
                var roleName = role?.Rolename ?? "User";

                var settingsMessage = "👤*Account Information:*\n\n" +
                                     $" - Username: *{user.Username ?? "Not set"}*\n" +
                                     $" - Telegram code: *{user.Telegramid}*\n" +
                                     $" - Join date: *{user.Joindate?.ToString("dd/MM/yyyy") ?? "N/A"}*\n" +
                                     $" - Status: *{(user.Isactive == true ? "Active" : "Inactive")}*\n" +
                                     $" - Role: *{roleName}*\n" +
                                     $" - Onchain ID: *{user.Onchainid ?? "Not set"}*\n\n" +
                                     "You can update your Onchain Id and participation role by selecting the edit buttons below.\n";

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("🐙 Onchain ID", "update_onchain"), InlineKeyboardButton.WithCallbackData("🐙Role", "update_role") }
                });

                await _botClient.SendTextMessageAsync(chatId, settingsMessage, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken, parseMode: ParseMode.Markdown);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching settings for Telegram ID: {TelegramId}", chatId);
                return "Error retrieving your account information.";
            }
        }

        private async Task<string> StartUpdateOnchainProcess(long chatId, CancellationToken cancellationToken)
        {
            lock (_feedbackState)
            {
                _feedbackState[chatId] = ("awaiting_onchainid", string.Empty);
            }
            await _botClient.SendTextMessageAsync(chatId, "💻 Please enter your new Onchain ID:", cancellationToken: cancellationToken);
            return string.Empty;
        }

        private async Task<string> UpdateOnchainId(long chatId, string newOnchainId, CancellationToken cancellationToken)
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

                    lock (_feedbackState)
                    {
                        _feedbackState.Remove(chatId);
                    }
                    await _botClient.SendTextMessageAsync(chatId, "🐳 *Onchain ID updated successfully!*\n 🐳Use */s* to view your updated information.", cancellationToken: cancellationToken);
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Onchain ID for Telegram ID: {TelegramId}", chatId);
                await _botClient.SendTextMessageAsync(chatId, "Error updating Onchain ID. Please try again.", cancellationToken: cancellationToken);
                return "Error";
            }
        }

        private async Task<string> StartUpdateRoleProcess(long chatId, CancellationToken cancellationToken)
        {
            var roleKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("🧑‍🏫Drep", "role_2"), InlineKeyboardButton.WithCallbackData("🧑‍💼User", "role_3") },
                new[] { InlineKeyboardButton.WithCallbackData("👑SPO", "role_4"), InlineKeyboardButton.WithCallbackData("💰Holder", "role_5") },
                new[] { InlineKeyboardButton.WithCallbackData("🧑‍⚖️Committee", "role_6") }
            });

            await _botClient.SendTextMessageAsync(chatId, "💻 Please select your new role:", replyMarkup: roleKeyboard, cancellationToken: cancellationToken);
            return string.Empty;
        }

        private async Task<string> UpdateRole(long chatId, int? roleId, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var user = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync((int)chatId);

                if (user != null)
                {
                    user.Roleid = roleId;
                    await unitOfWork.userReponsitory.UpdateEntity(user);
                    await unitOfWork.CompleteAsync();

                    await _botClient.SendTextMessageAsync(chatId, "🐳 *Role updated successfully!*\n🐳 Use */s* to view your updated information.", cancellationToken: cancellationToken, parseMode: ParseMode.Markdown);
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role for Telegram ID: {TelegramId}", chatId);
                await _botClient.SendTextMessageAsync(chatId, "Error updating role. Please try again.", cancellationToken: cancellationToken);
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
            await Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var questions = await unitOfWork.filterReponsitory.GetAllAsync();

                    if (questions == null || !questions.Any())
                    {
                        await _botClient.SendTextMessageAsync(chatId, "No suggested questions available.", cancellationToken: cancellationToken);
                        return;
                    }

                    const int itemsPerPage = 10;
                    int totalPages = (int)Math.Ceiling(questions.Count / (double)itemsPerPage);
                    page = Math.Max(1, Math.Min(page, totalPages));
                    lock (_filterPageState)
                    {
                        _filterPageState[chatId] = page;
                    }

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
                    await _botClient.SendTextMessageAsync(chatId, messageText.ToString(), parseMode: ParseMode.Markdown, replyMarkup: keyboard, cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling filter command for Telegram ID: {TelegramId}", chatId);
                    await _botClient.SendTextMessageAsync(chatId, "Error loading suggested questions.", cancellationToken: cancellationToken);
                }
            }, cancellationToken);
            return string.Empty;
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
                    await _botClient.SendTextMessageAsync(chatId, "Invalid question selected.", cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling filter question selection for Telegram ID: {TelegramId}", chatId);
                await _botClient.SendTextMessageAsync(chatId, "Error processing your question.", cancellationToken: cancellationToken);
            }
            return string.Empty;
        }
        #endregion

        #region Gọi lệnh Help Message
        private async Task<string> GetHelpMessage(long chatId, CancellationToken cancellationToken)
        {
            var helpMessage =
                "🌟 *Please select an option:*\n\n" +
                "👤 - *Settings*: _Account Settings_\n" +
                "💡 - *Filters*: _Recommended Questions_\n" +
                "📝 - *Feedback*: _Submit Feedback_\n" +
                "🏆 - *Score*: _View Achievements_\n\n" +
                "Or you can use the following commands:\n\n" +
                "❓ */h* - _Show available commands_\n" +
                "👤 */s* - _Account settings_\n" +
                "💡 */find* - _Recommended questions_\n" +
                "📝 */f* - _Send feedback_\n" +
                "🏆 */p* - _View achievements_\n\n" +
                "You can join our community group at: *[Cardano_ECO_VN](https://t.me/Cardano_ECO_VN)*";

            return helpMessage; // Chỉ trả về nội dung, không gửi tin nhắn
        }

        private InlineKeyboardMarkup GetHelpInlineKeyboard()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("👤 Settings", "settings"), InlineKeyboardButton.WithCallbackData("💡 Filter", "filter") },
                new[] { InlineKeyboardButton.WithCallbackData("📝 Feedback", "feedback"), InlineKeyboardButton.WithCallbackData("🏆 Point", "point") }
            });
        }
        #endregion

        #region Add callback query handler
        private async Task HandleCallbackQuery(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            var callbackData = callbackQuery.Data;

            var responseTask = callbackData switch
            {
                "settings" => GetSettingsMessage(chatId, cancellationToken),
                "filter" => HandleFilterCommand(chatId, cancellationToken),
                "feedback" => Task.FromResult("Please provide feedback with the command. Example: /f This is my feedback"),
                "point" => Task.FromResult("🏆 Your Achievement Points"),
                "update_onchain" => StartUpdateOnchainProcess(chatId, cancellationToken),
                "update_role" => StartUpdateRoleProcess(chatId, cancellationToken),
                var data when data.StartsWith("role_") => UpdateRole(chatId, int.Parse(data.Split('_')[1]), cancellationToken),
                var data when data.StartsWith("filter_page_") => HandleFilterCommand(chatId, cancellationToken, int.Parse(data.Split('_')[2])),
                var data when data.StartsWith("filter_question_") => HandleFilterQuestionSelection(chatId, int.Parse(data.Split('_')[2]), cancellationToken),
                _ => Task.FromResult("Invalid option")
            };

            var response = await responseTask;

            // Đảm bảo gửi phản hồi khi callbackData là "feedback"
            if (!string.IsNullOrEmpty(response))
            {
                await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
                await _botClient.SendTextMessageAsync(chatId, response, cancellationToken: cancellationToken);
            }
            else
            {
                await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
            }
        }
        #endregion

        #region Gọi API AI Model
        private async Task<string> GetResponseFromAI(long chatId, string message, CancellationToken cancellationToken)
        {
            // Tạm ngưng kết nối đến API, trả về phản hồi mặc định "Thank you"
            _logger.LogWarning("AI API connection suspended. Returning default response for chat {ChatId}", chatId);
            return "Thank you";
        }

        private async Task CreateChatHistoryAsync(long telegramId, string message, string? aiResponse, decimal? responseTime, CancellationToken cancellationToken)
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
    }
}