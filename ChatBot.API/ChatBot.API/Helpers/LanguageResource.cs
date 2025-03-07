namespace ChatBot.API.Helpers;

public static class LanguageResource
{
    private static readonly Dictionary<string, Dictionary<string, string>> _translations = new()
    {
        ["en"] = new Dictionary<string, string>
        {
            ["WelcomeMessage"] = "Hi *{0}*, Welcome to *GovernCardanoBot*!\n\n" +
                                "📖 *GovernCardanoBot* is an intelligent virtual assistant powered by  BBO Model, designed to answer questions related to the Cardano blockchain and its governance activities.\n\n" +
                                "🌟 *Please select an option:*\n\n" +
                                "👤 - *Settings*: _Account Settings_\n" +
                                "💡 - *Filters*: _Recommended Questions_\n" +
                                "📝 - *Feedback*: _Submit Feedback_\n" +
                                "🏆 - *Score*: _View Achievements_\n" +
                                "🌐 - *Language*: _Settings Language_\n\n" +
                                "Or you can use the following commands:\n\n" +
                                "❓ */h* - _Show available commands_\n" +
                                "📖 */la* - _Settings language_\n" +
                                "👤 */s* - _Account settings_\n" +
                                "💡 */find* - _Recommended questions_\n" +
                                "📝 */f* - _Send feedback_\n" +
                                "🏆 */p* - _View achievements_\n\n" +
                                "You can join our community group at: [Cardano_ECO_VN](https://t.me/Cardano_ECO_VN)",
            ["HelpMessage"] = "🌟 *Please select an option:*\n\n" +
                              "👤 - *Settings*: _Account Settings_\n" +
                              "💡 - *Filters*: _Recommended Questions_\n" +
                              "📝 - *Feedback*: _Submit Feedback_\n" +
                              "🏆 - *Score*: _View Achievements_\n" +
                              "🌐 - *Language*: _Settings Language_\n\n" +
                              "Or you can use the following commands:\n\n" +
                              "❓ */h* - _Show available commands_\n" +
                              "📖 */la* - _Settings language_\n" +
                              "👤 */s* - _Account settings_\n" +
                              "💡 */find* - _Recommended questions_\n" +
                              "📝 */f* - _Send feedback_\n" +
                              "🏆 */p* - _View achievements_\n\n" +
                              "You can join our community group at: [Cardano_ECO_VN](https://t.me/Cardano_ECO_VN)",
            ["SettingsButton"] = "👤 Settings",
            ["FilterButton"] = "💡 Filter",
            ["FeedbackButton"] = "📝 Feedback",
            ["PointButton"] = "🏆 Point",
            ["LanguageButton"] = "🌐 Language",
            ["SettingsMessage"] = "👤*Account Information:*\n\n" +
                                  " - Username: *{0}*\n" +
                                  " - Telegram code: *{1}*\n" +
                                  " - Join date: *{2}*\n" +
                                  " - Status: *{3}*\n" +
                                  " - Role: *{4}*\n" +
                                  " - Onchain ID: *{5}*\n" +
                                  " - Language: *{6}*\n\n" +
                                  "You can update your Onchain Id and participation role by selecting the edit buttons below.\n",
            ["OnchainIdButton"] = "🐙 Onchain ID",
            ["RoleButton"] = "🐙 Role",
            ["LanguagePrompt"] = "Please choose your language:",
            ["LanguageUpdated"] = "🌟 Language has been switched to English!",
            ["LanguageError"] = "Error updating language. Please try again.",
            ["FeedbackPrompt"] = "Please provide feedback with the command. Example: /f This is my feedback",
            ["FeedbackThanks"] = "Thank you for your feedback! 💖",
            ["PointMessage"] = "🏆 Your Achievement Points",
            ["NoUser"] = "User not found. Please use /start to register.",
            ["AIError"] = "Sorry, an error occurred while processing your request.",
            ["InvalidOption"] = "Invalid option",
            ["OnchainIdPrompt"] = "💻 Please enter your new Onchain ID:",
            ["OnchainIdSuccess"] = "🐳 *Onchain ID updated successfully!*\n 🐳Use */s* to view your updated information.",
            ["OnchainIdError"] = "Error updating Onchain ID. Please try again.",
            ["SelectNewRole"] = "💻 Please select your new role:",
            ["RoleSuccess"] = "🐳 *Role updated successfully!*\n 🐳Use */s* to view your updated information.",
            // Các key cho lệnh
            ["Command_Start"] = "Start the bot",
            ["Command_H"] = "Show available commands",
            ["Command_S"] = "Account settings",
            ["Command_Find"] = "Recommended questions",
            ["Command_F"] = "Send feedback",
            ["Command_P"] = "View achievements",
            ["Command_La"] = "Settings language"


        },
        ["vi"] = new Dictionary<string, string>
        {
            ["WelcomeMessage"] = "Xin chào *{0}*, Chào mừng bạn đến với *GovernCardanoBot*!\n\n" +
                                "📖 *GovernCardanoBot* là trợ lý ảo thông minh được hỗ trợ bởi BBO Model, được thiết kế để trả lời các câu hỏi liên quan đến blockchain Cardano và các hoạt động quản trị của nó.\n\n" +
                                "🌟 *Vui lòng chọn một tùy chọn:*\n\n" +
                                "👤 - *Cài đặt*: _Thiết lập tài khoản_\n" +
                                "💡 - *Bộ lọc*: _Câu hỏi gợi ý_\n" +
                                "📝 - *Phản hồi*: _Gửi đánh giá_\n" +
                                "🏆 - *Điểm*: _Xem thành tích_\n" +
                                "🌐 - *Ngôn ngữ*: _Thiết lập ngôn ngữ_\n\n" +
                                "Hoặc bạn có thể sử dụng các lệnh sau:\n\n" +
                                "❓ */h* - _Hiển thị các lệnh có sẵn_\n" +
                                "📖 */la* - _Thiết lập ngôn ngữ_\n" +
                                "👤 */s* - _Thiếp lập tài khoản_\n" +
                                "💡 */find* - _Câu hỏi gợi ý_\n" +
                                "📝 */f* - _Gửi đánh giá_\n" +
                                "🏆 */p* - _Xem thành tích_\n\n" +
                                "Bạn có thể tham gia nhóm cộng đồng tại: [Cardano_ECO_VN](https://t.me/Cardano_ECO_VN)",
            ["HelpMessage"] = "🌟 *Vui lòng chọn một tùy chọn:*\n\n" +
                              "👤 - *Cài đặt*: _Thiết lập tài khoản_\n" +
                              "💡 - *Bộ lọc*: _Câu hỏi gợi ý_\n" +
                              "📝 - *Phản hồi*: _Gửi đánh giá_\n" +
                              "🏆 - *Điểm*: _Xem thành tích_\n" +
                              "🌐 - *Ngôn ngữ*: _Thiết lập ngôn ngữ_\n\n" +
                              "Hoặc bạn có thể sử dụng các lệnh sau:\n\n" +
                              "❓ */h* - _Hiển thị các lệnh có sẵn_\n" +
                              "📖 */la* - _Thiết lập ngôn ngữ_\n" +
                              "👤 */s* - _Thiếp lập tài khoản_\n" +
                              "💡 */find* - _Câu hỏi gợi ý_\n" +
                              "📝 */f* - _Gửi đánh giá_\n" +
                              "🏆 */p* - _Xem thành tích_\n\n" +
                              "Bạn có thể tham gia nhóm cộng đồng tại: [Cardano_ECO_VN](https://t.me/Cardano_ECO_VN)",
            ["SettingsButton"] = "👤 Cài đặt",
            ["FilterButton"] = "💡 Bộ lọc",
            ["FeedbackButton"] = "📝 Phản hồi",
            ["PointButton"] = "🏆 Điểm",
            ["LanguageButton"] = "🌐 Ngôn ngữ",
            ["SettingsMessage"] = "👤*Thông tin tài khoản:*\n\n" +
                                  " - Tên người dùng: *{0}*\n" +
                                  " - Mã Telegram: *{1}*\n" +
                                  " - Ngày tham gia: *{2}*\n" +
                                  " - Trạng thái: *{3}*\n" +
                                  " - Vai trò: *{4}*\n" +
                                  " - Onchain ID: *{5}*\n" +
                                  " - Ngôn ngữ sử dụng: *{6}*\n\n" +
                                  "Bạn có thể cập nhật Onchain ID và vai trò tham gia bằng cách chọn các nút chỉnh sửa bên dưới.\n",
            ["OnchainIdButton"] = "🐙 Onchain ID",
            ["RoleButton"] = "🐙 Vai trò",
            ["LanguagePrompt"] = "Vui lòng chọn ngôn ngữ bạn sử dụng:",
            ["LanguageUpdated"] = "🌟 Ngôn ngữ đã được chuyển sang tiếng Việt!",
            ["LanguageError"] = "Lỗi khi cập nhật ngôn ngữ. Vui lòng thử lại.",
            ["FeedbackPrompt"] = "Vui lòng cung cấp phản hồi bằng lệnh. Ví dụ: /f Đây là đánh giá của tôi",
            ["FeedbackThanks"] = "Cảm ơn bạn đã gửi phản hồi! 💖",
            ["PointMessage"] = "🏆 Điểm thành tích của bạn",
            ["NoUser"] = "Không tìm thấy người dùng. Vui lòng dùng /start để đăng ký.",
            ["AIError"] = "Xin lỗi, đã xảy ra lỗi khi xử lý yêu cầu của bạn.",
            ["InvalidOption"] = "Tùy chọn không hợp lệ",
            ["OnchainIdPrompt"] = "💻 Vui lòng nhập Onchain ID mới của bạn:",
            ["OnchainIdSuccess"] = "🐳 *Onchain ID đã được cập nhật thành công!*\n 🐳Dùng */s* để xem thông tin đã cập nhật.",
            ["OnchainIdError"] = "Lỗi khi cập nhật Onchain ID. Vui lòng thử lại.",
            ["SelectNewRole"] = "💻 Vui lòng chọn vai trò bạn trên Onchain:",
            ["RoleSuccess"] = "🐳 *Cập nhật vai trò thành công.*\n 🐳Sử dụng lệnh */s* để xem thông tin thay đổi của bạn!",

          
        }
    };

    public static string GetTranslation(string language, string key, params object[] args)
    {
        if (_translations.TryGetValue(language, out var translations) && translations.TryGetValue(key, out var value))
        {
            return string.Format(value, args);
        }
        // Fallback to English if language or key not found
        return _translations["en"].TryGetValue(key, out var fallback) ? string.Format(fallback, args) : key;
    }

}
