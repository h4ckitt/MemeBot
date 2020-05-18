using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using File = System.IO.File;

namespace MemeBot.Telegram_Client
{
    
    public static class Tg
    {
        private static ITelegramBotClient Bot;
        const string Token = "982750050:AAHtiha5647ulyjp9o_6Z-yoUqCsbuIjdyo";
        //private static string _caption = null;
        public static void Start()
        {
            Bot=new TelegramBotClient(Token);
            var me = Bot.GetMeAsync().Result;
            Console.WriteLine($"Hello, I'm {me.Id} And My Name Is {me.FirstName}");
            Bot.OnMessage += OnMessage;
            Bot.StartReceiving();
            Thread.Sleep(int.MaxValue);
        }

        static async void OnMessage(object sender, MessageEventArgs e)
        {

            if (e.Message.Type == MessageType.Text)
            {
                if (File.Exists($"{e.Message.Chat.Id}"))
                {
                    string caption = e.Message.Text;
                    if (e.Message.Text.Split(' ').First().ToLower() == "/no")
                    {
                        await Bot.SendTextMessageAsync(e.Message.Chat, "Saving Picture Without Caption");
                        caption = null;
                    }
                    {
                        StreamReader reader = new StreamReader($"{e.Message.Chat.Id}");
                        while (reader.EndOfStream == false)
                        {
                            string path = await reader.ReadLineAsync();
                            string img = await reader.ReadLineAsync();
                            DownloadFile(img, path, caption);
                            reader.Close();
                            File.Delete($"{e.Message.Chat.Id}");
                            return;
                        }
                    }
                }
                
                Console.WriteLine("Here");
                Console.WriteLine(e.Message.Text.Split(' ').First());
                switch (e.Message.Text.Split(' ').First())
                {
                    case "/count":
                        Console.WriteLine("hola");
                        var files = Directory
                            .EnumerateFiles("/home/dharmy/Pictures", "*.*", SearchOption.AllDirectories)
                            .Where(s => s.EndsWith(".jpg",StringComparison.OrdinalIgnoreCase)||s.EndsWith(".png",StringComparison.OrdinalIgnoreCase));
                        int count = 0;
                        foreach (var file in files) count++;
                        await Bot.SendTextMessageAsync(e.Message.Chat,$"There Are {count} Memes Available Rn");
                        break;
                    
                    default:
                        await Bot.SendTextMessageAsync(e.Message.Chat,$"I Don't Reallly Understand That Holmes.");
                        break;
                }
            }
            if (e.Message.Type == MessageType.Photo)
            {
                string path = $"/home/dharmy/Pictures/{PhotoName()}.jpg";
                Console.WriteLine(path);
                if (!string.IsNullOrEmpty(e.Message.Caption))
                {
                    DownloadFile(e.Message.Photo.LastOrDefault()?.FileId, path, e.Message.Caption);
                    await Bot.SendTextMessageAsync(e.Message.Chat, "Downloaded");
                    return;
                }
                await Bot.SendTextMessageAsync(e.Message.Chat, "Send A Caption Or /no For Empty Caption");
                await File.Create($"{e.Message.Chat.Id}").DisposeAsync();
                await using StreamWriter writer = new StreamWriter($"{e.Message.Chat.Id}");
                await writer.WriteLineAsync(path);
                await writer.WriteLineAsync(e.Message.Photo.LastOrDefault()?.FileId);
                writer.Close();
            }
        }

        private static string PhotoName()
        {
            var random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzKLMNOPQRSTUVWXYZ0123456789";
            return new string(value: Enumerable.Repeat(chars,12).Select(s=>s[random.Next(s.Length)]).ToArray());
        }

        private static async void DownloadFile(string e, string path, string caption)
        {
            try
            {
                Console.WriteLine(caption);
                 StreamWriter write = new StreamWriter($"/home/dharmy/Documents/config.txt",true);
                var img = await Bot.GetFileAsync(e);
                Console.WriteLine(img.FileSize);
                Console.WriteLine(img.FilePath);
                string dUrl = $"https://api.telegram.org/file/bot{Token}/{img.FilePath}";
                using (WebClient net = new WebClient())
                {
                    net.DownloadFileAsync(new Uri(dUrl), path);
                }
                if (! string.IsNullOrEmpty(caption))
                    using (write)
                    {
                        await write.WriteLineAsync(path + " - " + caption);
                    }

                Console.WriteLine("Downloaded");
            }

            catch (Exception x)
            {
                Console.WriteLine("Error Getting File: "+x.Message);
            }
        }

    }
}