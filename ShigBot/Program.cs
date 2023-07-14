using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using ShigBot.Commands;
using DSharpPlus.SlashCommands;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.Entities;

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

            var services = BuildServiceProvider();
            var slash = discord.UseSlashCommands(new SlashCommandsConfiguration
            {
                Services = services
            });

            slash.RegisterCommands<SlashCommands>();

            slash.SlashCommandErrored += async (sender, eventArgs) =>
            {
                // Handle the command error
                await HandleCommandError(eventArgs.Context, eventArgs.Exception);
            };
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "$" }
            });
            commands.RegisterCommands<MainModule>();
            commands.RegisterCommands<Diceroller>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddSingleton<SlashDiceroller>(); // Register SlashDiceroller as a service

            return services.BuildServiceProvider();
        }

        // Method to handle the command error
        private static async Task HandleCommandError(InteractionContext context, Exception exception)
        {
            // Log the error or perform any desired actions
            Console.WriteLine($"Command execution error: {exception.Message}");

            // Respond to the user with an error message
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("An error occurred while executing the command.")
                .AsEphemeral(true));
        }
    }

    public class SlashCommands : ApplicationCommandModule
    {
        private readonly SlashDiceroller _diceroller;

        public SlashCommands(SlashDiceroller diceroller)
        {
            _diceroller = diceroller;
        }

        [SlashCommand("roll", "Rolls the dice. Use help for more information.")]
        public async Task RollCommand(InteractionContext ctx, [Option("input", "The input for the roll command.")] string input)
        {
            await _diceroller.RollCommand(ctx, input);
        }
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
        string result = await _diceroller.RollCommand(ctx, option);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(result));
    }
}
