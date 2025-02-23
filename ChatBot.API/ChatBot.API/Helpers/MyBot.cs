namespace ChatBot.API.Helpers;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Args;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net.Http;
using System.Text.Json;
using System.Text;
public class MyBot : AutoCleanForm
{
    //public override async Task Load(MessageResult message)
    //{
    //    await message.ConfirmAction();

    //    string userMessage = message.MessageText;
    //    long chatId = message.ChatId;

    //    if (userMessage == "/start")
    //    {
    //        var btn = new ButtonForm();
    //        btn.AddButtonRow(new ButtonBase("Ask AI", "ask_ai"));

    //        await Device.Send("Welcome! Click below to ask AI:", btn);
    //    }
    //    else
    //    {
    //        string aiResponse = await GetResponseFromAI(chatId, userMessage);
    //        await Device.Send(aiResponse);
    //    }
    //}

    //public override async Task ButtonClicked(ButtonClickedEventArgs e)
    //{
    //    if (e.Button == null) return;

    //    long chatId = e.Device.ChatId;

    //    if (e.Button.Value == "ask_ai")
    //    {
    //        await e.Device.Send("Send me your question.");
    //    }
    //}

    private async Task<string> GetResponseFromAI(long chatId, string message)
    {
        using var httpClient = new HttpClient();
        string apiUrl = "http://aitreviet.duckdns.org:8000";

        var payload = new { ID = chatId, message = message };
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync(apiUrl, jsonContent);
        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            return doc.RootElement.GetProperty("answer").GetString();
        }
        else
        {
            return "AI service is unavailable at the moment.";
        }
    }
}
