using BudgetHistory.Logging.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BudgetHistory.Logging
{
    public class TelegramLogger : ICustomLogger
    {
        private readonly string _token;
        private readonly string _chatId;

        public TelegramLogger(IConfiguration config)
        {
            _token = config.GetSection("TelegramToken").Value;
            _chatId = config.GetSection("TelegramBotMyChat").Value;
        }

        public async Task LogError(string errorMessage)
        {
            await SendTelegramMessageAsync($"‼️ {errorMessage}");
        }

        public async Task LogInfo(string infoMessage)
        {
            await SendTelegramMessageAsync($"⚠️ {infoMessage}");
        }

        private async Task SendTelegramMessageAsync(string message)
        {
            var requestUrl = new Uri($"https://api.telegram.org/bot{_token}/sendMessage?chat_id={_chatId}&text={message}");
            var client = new HttpClient();
            var response = await client.GetAsync(requestUrl);
        }
    }
}