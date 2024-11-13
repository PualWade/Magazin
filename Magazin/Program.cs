using Magazin.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Magazin.Services;

namespace Magazin
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var token = Environment.GetEnvironmentVariable("TOKEN") ?? "YOUR_BOT_TOKEN";

            var botManager = new BotManager(token);
            await botManager.StartAsync();

            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();

            await botManager.StopAsync();
        }
    }
}
