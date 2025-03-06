using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ChatBot.API.Helpers;
using ChatBot.API.Interface;

namespace ChatBot.API.Handle
{
    public class BotCommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<BotCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public BotCommandHandler(ITelegramBotClient botClient, ILogger<BotCommandHandler> logger, IUnitOfWork unitOfWork)
        {
            _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task SetBotCommandsAsync(CancellationToken cancellationToken)
        {
            // Lệnh duy nhất với mô tả tiếng Anh
            var commands = new[]
            {
                new BotCommand { Command = "start", Description = LanguageResource.GetTranslation("en", "Command_Start") },
                new BotCommand { Command = "h", Description = LanguageResource.GetTranslation("en", "Command_H") },
                new BotCommand { Command = "s", Description = LanguageResource.GetTranslation("en", "Command_S") },
                new BotCommand { Command = "find", Description = LanguageResource.GetTranslation("en", "Command_Find") },
                new BotCommand { Command = "f", Description = LanguageResource.GetTranslation("en", "Command_F") },
                new BotCommand { Command = "p", Description = LanguageResource.GetTranslation("en", "Command_P") },
                new BotCommand { Command = "la", Description = LanguageResource.GetTranslation("en", "Command_La") }
            };

            try
            {
                // Đặt lệnh mặc định (tiếng Anh) cho tất cả người dùng
                await _botClient.SetMyCommandsAsync(commands, scope: new BotCommandScopeAllPrivateChats(), cancellationToken: cancellationToken);
                _logger.LogInformation("Bot commands updated successfully with English descriptions.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bot commands.");
            }
        }
    }
}