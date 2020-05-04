using System.IO;
using MemeBot.Telegram_Client;
namespace MemeBot
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!File.Exists($"/home/dharmy/Pictures/Config.txt"))
                File.Create($"/home/dharmy/Pictures/Config.txt");
            Tg.Start();
        }
    }
}