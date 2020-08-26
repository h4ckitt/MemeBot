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
    
    public static class Tg
    {
        private static ITelegramBotClient Bot;
        private const string Token = "<Token Here>";
        public static async void Start()
        {            
            Bot=new TelegramBotClient(Token);    
            if (!File.Exists($"/root/memeler/config.csv")) {                
                StreamWriter write = new StreamWriter($"/root/memeler/config.csv",true);
                await write.WriteLineAsync("Full");
                write.Close();
            }
            var me = Bot.GetMeAsync().Result;
            Console.WriteLine($"Hello, I'm {me.Id} And My Name Is {me.FirstName}");
            Bot.OnMessage += OnMessage;
            try {
            Bot.StartReceiving();
            Thread.Sleep(int.MaxValue);
        }
        	catch (AggregateException ex) {
        		Console.WriteLine(ex.Message+":\n");
        		Console.WriteLine("Bad Network Connection Or Token.");
        	}
        }

        static async void OnMessage(object sender, MessageEventArgs e)
        {
        	StreamWriter logger = new StreamWriter($"/root/memeler/logs.txt", true);
            if (e.Message.Chat.Id == 502979049 || e.Message.Chat.Id == 513085689)
            {
                if (e.Message.Type == MessageType.Text)
                {
                    if (File.Exists($"{e.Message.Chat.Id}"))
                    {
                        string caption = e.Message.Text;
                        if (e.Message.Text.ToLower() == "/no")
                        {
                            await Bot.SendTextMessageAsync(e.Message.Chat, "Saving Picture Without Caption");
                            caption = null;
                        }

                        {
                            StreamReader reader = new StreamReader($"{e.Message.Chat.Id}");
                            while (reader.EndOfStream == false)
                            {
                                string imagename = await reader.ReadLineAsync();
                                string img = await reader.ReadLineAsync();
                                await logger.WriteLineAsync("Caption: "+ caption);
                                DownloadFile(img, imagename, caption);
                                await Bot.SendTextMessageAsync(e.Message.Chat, "File Downloaded");
                                reader.Close();
                                File.Delete($"{e.Message.Chat.Id}");
                                return;
                            }
                        }
                    }

                    switch (e.Message.Text.Split(' ').First())
                    {
                        case "/count":
                            var files = Directory
                                .EnumerateFiles("/root/memeler/images", "*.*", SearchOption.AllDirectories)
                                .Where(s => s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                            s.EndsWith(".png", StringComparison.OrdinalIgnoreCase));
                            int count = 0;
                            foreach (var file in files) count++;
                            await Bot.SendTextMessageAsync(e.Message.Chat, $"There Are {count} Memes Available Rn");
                            break;
                        case "/start":
                        case "/help":
                            string help = $"Hello {e.Message.Chat.FirstName}, " +
                                          "You Don't Need A Help Text.";
                            await Bot.SendTextMessageAsync(e.Message.Chat, help);
                            break;
                        default:
                        	await logger.WriteLineAsync("Unknown Command "+ e.Message.Text +" Sent.");
                            await Bot.SendTextMessageAsync(e.Message.Chat, $"I Don't Really Understand That Holmes.");
                            break;
                    }
                }

                if (e.Message.Type == MessageType.Photo)
                {                

                    await logger.WriteLineAsync("Message Received From " + e.Message.Chat.FirstName + " With Chat ID " + e.Message.Chat.Id +":");

                    string imagename = $"{PhotoName()}.jpg";

                    await logger.WriteLineAsync("Picture File Name: " + imagename);
                    if (!string.IsNullOrEmpty(e.Message.Caption))
                    {
                    	await logger.WriteLineAsync("Caption: " + e.Message.Caption);
                        DownloadFile(e.Message.Photo.LastOrDefault()?.FileId, imagename, e.Message.Caption);
                        await Bot.SendTextMessageAsync(e.Message.Chat, "File Downloaded");
                        return;
                    }

                    await Bot.SendTextMessageAsync(e.Message.Chat, "Send A Caption Or /no For Empty Caption");
                    await File.Create($"{e.Message.Chat.Id}").DisposeAsync();
                    await using StreamWriter writer = new StreamWriter($"{e.Message.Chat.Id}");
                    await writer.WriteLineAsync(imagename);
                    await writer.WriteLineAsync(e.Message.Photo.LastOrDefault()?.FileId);
                    writer.Close();
                }
            }            
            else
            {
                await Bot.SendTextMessageAsync(e.Message.Chat.Id,$"Hello {e.Message.Chat.FirstName}, You " +
                                                                 $"Are Not Authorized To Use This Bot");
            }
            await logger.WriteLineAsync("\n");
            logger.Close();
        }

        private static string PhotoName()
        {
            var random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzKLMNOPQRSTUVWXYZ0123456789";
            return new string(value: Enumerable.Repeat(chars,12).Select(s=>s[random.Next(s.Length)]).ToArray());
        }

        private static async void DownloadFile(string e, string imagename, string caption)
        {
            try
            {
            	string path = "/root/memeler/images/" + imagename;    
                StreamWriter write = new StreamWriter($"/root/memeler/config.csv",true);                
                var img = await Bot.GetFileAsync(e);
                string dUrl = $"https://api.telegram.org/file/bot{Token}/{img.FilePath}";
                using (WebClient net = new WebClient())
                {
                    net.DownloadFileAsync(new Uri(dUrl), path);
                }                
                using (write)
                {
                    await write.WriteLineAsync(imagename + " | " + caption);
                }
                write.Close();
                Console.WriteLine("Filename: {0}\nCaption: {1}",imagename,caption);
                Console.WriteLine("Downloaded");
                Console.WriteLine("");
            }

            catch (Exception x)
            {
            	using (StreamWriter log = new StreamWriter($"/root/memeler/logs.txt", true))
            	{
            		await log.WriteLineAsync("Error Getting File: "+ imagename);
            		await log.WriteLineAsync("Error Message: "+ x.Message);
            		log.Close();
            	}
                Console.WriteLine("Error Getting File: "+x.Message);
            }
        }

    }
}
