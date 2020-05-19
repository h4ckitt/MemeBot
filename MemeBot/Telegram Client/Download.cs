using System.IO;

namespace MemeBot.Telegram_Client
{
    public static class Download
    {
        static string getToken()
        {
            using (StreamReader reader = new StreamReader($"/home/dharmy/Documents/Token.txt"))
            {
                return reader.ReadLine();
            }
        }
    }
}