using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using ShigBot.Commands;
using DSharpPlus.SlashCommands;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ShigiraBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                //Top token is live build, bottom token is Test. Make sure swapped before build!

                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });

            var slash = discord.UseSlashCommands();

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "$" }
            });

            slash.RegisterCommands<SlashCommands>();
            commands.RegisterCommands<MainModule>();
            commands.RegisterCommands<Diceroller>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }

    public class SlashCommands : ApplicationCommandModule
    {
        private readonly global::SlashDiceroller _diceroller;

        public SlashCommands(global::SlashDiceroller diceroller)
        {
            _diceroller = diceroller;
        }

        [SlashCommand("roll", "Rolls the dice. Use help for more information.")]
        public async Task RollCommand(InteractionContext ctx, [Option("Input", "The input for the roll command. Can be standard format or 'help'.")] string option)
        {
            if (ctx is null) Console.WriteLine("test");
            if (option is null) Console.WriteLine("test2");
            await _diceroller.RollCommand(ctx, option);
        }
    }
}
