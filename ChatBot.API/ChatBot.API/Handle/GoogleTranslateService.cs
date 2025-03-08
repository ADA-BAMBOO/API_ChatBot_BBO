
using Google.Cloud.Translation.V2;
using Microsoft.Extensions.Configuration;
namespace ChatBot.API.Handle;

public class GoogleTranslateService
{
    private readonly TranslationClient _client;
    public GoogleTranslateService(IConfiguration configuration)
    {
        var apiKey = configuration["GoogleTranslate:ApiKey"];
        _client = TranslationClient.CreateFromApiKey(apiKey);
    }

    public async Task<string> DetectAndTranslateToEnglishAsync(string text)
    {
        var detection = await _client.DetectLanguageAsync(text);
        if (detection.Language == "en")
            return text; // Không cần dịch nếu đã là tiếng Anh
        return await TranslateToEnglishAsync(text);
    }

    public async Task<string> TranslateToEnglishAsync(string text)
    {
        var response = await _client.TranslateTextAsync(text, "en");
        return response.TranslatedText;
    }

    public async Task<string> TranslateToVietnameseAsync(string text)
    {
        var response = await _client.TranslateTextAsync(text, "vi");
        return response.TranslatedText;
    }
}
