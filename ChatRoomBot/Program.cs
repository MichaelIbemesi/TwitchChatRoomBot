using System;

namespace ChatRoomBot
{
    class Program
    {
        static void Main(string[] args)
        {
            TwitchBot twitchBot = new TwitchBot();

            twitchBot.Connect(true);

            Console.ReadLine();

            twitchBot.Disconnect();
        }
    }
}
