using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System;
using System.Net.Http.Json;
using System.Collections.Generic;
using EMTestTask.Models;


namespace EMTestTask.Services
{
    public class StatisticBotSetting
    {
        public bool Enable { get; set; } = false;
        public string Token { get; set; } = string.Empty;
        public long ChatId { get; set; } = 0;
    }

    public class StatisticBotService(
        ILogger<StatisticBotService> logger,
        IOptions<StatisticBotSetting> settings
        )
    {
        private readonly ILogger _logger = logger;
        private readonly StatisticBotSetting _settings = settings.Value;

        private void SendMessage(string msg)
        {
            if (!_settings.Enable)
            {
                return;
            }
            try
            {
                var channelId = _settings.ChatId;
                if (channelId > 0)
                {
                    channelId = long.Parse("-100" + channelId);
                }

                var payload = new
                {
                    chat_id = channelId,
                    text = msg,
                    parse_mode = "Markdown"
                };

                var url = $"https://api.telegram.org/bot{_settings.Token}/sendMessage";
                var http = new HttpClient();
                var response = http.PostAsJsonAsync(url, payload).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                string errMsg = (e.Message ?? "No message") + "\n" + (e.StackTrace ?? "No stack trace");
                _logger.LogWarning("{Msg}", errMsg);
            }
        }

        public void UploadFile(AdvertisingServiceUpload result)
        {
            var msg = "📥 *Загрухка файла*\n\n";
            if (result.Result != 0)
            {
                msg += $"Ошибка файла: \n{result.Message}\n";
            }
            else
            {
                msg += $"Всего строк: {result.LinesTotal}\n";
                msg += $"С ошибками: {result.LinesWithErrors}\n";
            }
            SendMessage(msg);
        }

        public void Search(string request, IEnumerable<string>? result)
        {
            var msg = "🔍 *Поиск*\n\n";
            msg += $"Запрос: {request}\n";
            msg += $"Ответ: {String.Join(", ", result ?? [])}\n";
            SendMessage(msg);
        }

        public void Reset(AdvertisingServiceUpload result)
        {
            var msg = "📥 *Сброс*\n\n";
            if (result.Result != 0)
            {
                msg += $"Ошибка файла: \n{result.Message}\n";
            }
            else
            {
                msg += $"Всего строк: {result.LinesTotal}\n";
                msg += $"С ошибками: {result.LinesWithErrors}\n";
            }
            SendMessage(msg);
        }
    }
}
