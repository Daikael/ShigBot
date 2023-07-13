using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace ShigBot.Commands
{
    public class MainModule : BaseCommandModule
    {
        [Command("Guide")]
        public async Task GuideCommand(CommandContext ctx)
        {
            string basehelpmessage = "Greetings, this command is intended to help you.\n\n"
            + "For help with specific commands, please run the command and add help to the end, E.G.: $roll help\n"
            + "For a list of commands, please use $commands";
            await ctx.RespondAsync(basehelpmessage);
        }
        [Command("Commands")]
        public async Task CommandsCommand(CommandContext ctx)
        {
            string commandslistmessage = "The currently recorded list of commands are as follows.\n\n"
                + "Roll, A dice roller, will take a given input and give you a list of all rolls and their modifiers."
                + "Guide, A simple explanation on how to use commands."
                + "Commands, a list of commands and what they do.";
            await ctx.RespondAsync(commandslistmessage);
        }
    }
}
