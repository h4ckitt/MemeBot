using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

namespace MemeBot.Telegram_Client
{
    
    public class Tg
    {
        private static ITelegramBotClient Bot;
        const string Token = "982750050:AAHtiha5647ulyjp9o_6Z-yoUqCsbuIjdyo";
        static string _caption=null;
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
            if (e.Message.Type == MessageType.Photo)
            {
                if (!string.IsNullOrEmpty(e.Message.Caption))
                    _caption = e.Message.Caption;
                if (string.IsNullOrEmpty(_caption))
                {
                    await Bot.SendTextMessageAsync(e.Message.Chat, "Send A Caption With /caption Or Send Nil For No Caption");
                    
                }
                string path = $"/home/dharmy/Pictures/{PhotoName()}.jpg";
                Console.WriteLine(path);
                 DownloadFile(e,path);
            }   
        }

        private static string PhotoName()
        {
            var random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzKLMNOPQRSTUVWXYZ0123456789";
            return new string(value: Enumerable.Repeat(chars,12).Select(s=>s[random.Next(s.Length)]).ToArray());
        }

        private static async void DownloadFile(MessageEventArgs e, string path)
        {
            try
            {
                var img = await Bot.GetFileAsync(e.Message.Photo.LastOrDefault()?.FileId);
                string dUrl = $"https://api.telegram.org/file/bot{Token}/{img.FilePath}";
                using (WebClient net = new WebClient())
                {
                    net.DownloadFileAsync(new Uri(dUrl), $"/home/dharmy/Pictures/{path}.jpg");
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