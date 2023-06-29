using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using ShigBot.Commands;

namespace ShigiraBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = "ODM2MDE3NTc2MDcyNTc3MDI0.GzYW-t.K4R-P9nFNjqk2x39vSgDaY8pGmeD9WKcFAYCxU",
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "$" }
            });

            commands.RegisterCommands<MainModule>();
            commands.RegisterCommands<Diceroller>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}