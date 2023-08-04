using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class SlashDiceroller : ApplicationCommandModule
{
    // Class-level variable for includeExpression
    public bool includeExpression = true;

    [Command("Roll")]
    public async Task<string> RollCommand(InteractionContext ctx, [RemainingText] string option)
    {
        //Console.WriteLine("Test2");
        if (option.Trim().Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            // Help message handling
            string helpMessage = "Dice Roller Help:\n\n"
                + "To roll dice, use the following format:\n"
                + "- Specify the number of dice rolls: `<number>d<sides>` (e.g., `2d6`)\n"
                + "- Add an optional loop multiplier for the number of times to roll subsequent dice (e.g., `2 2d6`)\n"
                + "- Add an optional adjustment to each roll: `i±<value>` (e.g., `i+1` for +1 adjustment)\n"
                + "- Add an optional adjustment to the end total: `o±<value>` (e.g., `o+2` for +2 adjustment)\n"
                + "- Add an optional adjustment to the group total: `g±<value>` (e.g., `g+2` for +2 adjustment)\n"
                + "- Do note, the optional adjustment values can be entered as negatives, (e.g., `i-2` or `o-8`)\n\n"
                + "Example: `3 2d6 i+1 o+2 g+3`\n\n";

            Console.WriteLine("Help message executed");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(helpMessage).AsEphemeral(true));
        }
        else
            //Console.WriteLine("test3");
            Console.WriteLine(option);
        {
            // Check if option ends with "-ex" to set includeExpression
            includeExpression = !option.Trim().EndsWith("-ex");

            bool success = false;

            try
            {
                string adjustedRoll = DiceRand(option);
                success = true;

                // Split the output into smaller chunks
                var chunks = SplitTextIntoChunks(adjustedRoll, option, ctx);

                var sb = new StringBuilder();
                foreach (var chunk in chunks)
                {
                    sb.AppendLine($"You have rolled: {chunk}.");
                }
                string result = sb.ToString().TrimEnd();
                //Console.WriteLine(result);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(result));
            }
            catch (FormatException ex)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Error: {ex.Message} option: {option}"));
            }
            finally
            {
                if (!success)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Invalid option format. Please enter a valid dX formula, e.g., \"3 2d6 i+1 o+2\"."));
                }
            }
        }

        // Return an empty string as a default value
        return "";
    }

    private IEnumerable<string> SplitTextIntoChunks(string text, string option, InteractionContext ctx)
    {
        const int maxChunks = 2; // Maximum number of chunks allowed
        const int maxChunkSize = 1900; // Maximum size of each chunk

        if (text.Length <= maxChunkSize)
        {
            return new List<string> { text }; // Return a single chunk if the text is within the limit
        }
        else
        {
            List<string> chunks = new List<string>();
            int chunkCount = 0;

            // Split the text into chunks based on the maximum chunk size
            for (int i = 0; i < text.Length; i += maxChunkSize)
            {
                string chunk = text.Substring(i, Math.Min(maxChunkSize, text.Length - i));
                chunks.Add(chunk);
                chunkCount++;

                if (chunkCount > maxChunks)
                {
                    // Log the error message
                    string userInfo = $"{ctx.User.Username}#{ctx.User.Discriminator} (ID: {ctx.User.Id})";
                    Console.WriteLine($"[WARNING] Exceeded maximum number of chunks. User: {userInfo}");
                    Console.WriteLine($"option from RollCommand: {option}");
                    // Return an empty list to indicate an error
                    throw new FormatException($"Illegal output length.");
                }
            }

            return chunks;
        }
    }

    public static string DiceRand(string option, bool exo = false, bool includeExpression = true)
    {
        // Overrides the value of includeexpression
        if (exo)
        {
            includeExpression = true;
        }
        // Trim leading and trailing whitespace from the option
        option = option.Trim();

        // Define the regular expression pattern
        string pattern = @"(?:(\d+)\s+)?(\d+)\s*d\s*(\d+)(?:\s*i([+\-]\d+))?(?:\s*o([+\-]\d+))?(?:\s*g([+\-]\d+))?";

        // Create a new Regex instance with the pattern and options
        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

        // Match the option string against the regex pattern
        MatchCollection matches = regex.Matches(option);

        if (matches.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            Random random = new Random();

            // Define the threshold for the number of rolls
            int maxRolls = 10; // Adjust this value as needed

            foreach (Match match in matches)
            {
                // Parse the captured groups
                string firstNumberGroup = match.Groups[1].Value;
                string numberGroup = match.Groups[2].Value;
                string sizeGroup = match.Groups[3].Value;
                string increaseGroup = match.Groups[4].Value;
                string increaseGroup2 = match.Groups[5].Value;
                string increaseGroup3 = match.Groups[6].Value;

                // Roll the dice based on the captured groups
                int firstNumber = string.IsNullOrEmpty(firstNumberGroup) ? 1 : int.Parse(firstNumberGroup);
                int number = int.Parse(numberGroup);
                int size = int.Parse(sizeGroup);

                int total = 0;

                if (firstNumber > maxRolls)
                {
                    // Set includeExpression to true
                    includeExpression = true;
                }

                for (int j = 0; j < firstNumber; j++)
                {
                    List<string> rollExpressions = new List<string>();
                    int rollSum = 0;

                    for (int i = 0; i < number; i++)
                    {
                        int roll = random.Next(1, size + 1);
                        int modifiedRoll = roll;

                        // Apply the adjustments to the roll
                        if (!string.IsNullOrEmpty(increaseGroup))
                        {
                            int increaseValue = int.Parse(increaseGroup);
                            modifiedRoll += increaseValue;
                        }

                        rollSum += modifiedRoll;
                        rollExpressions.Add($"{roll}({modifiedRoll})");
                    }

                    total += rollSum;

                    // Append the roll expressions to the output
                    if (includeExpression)
                    {
                        sb.AppendLine($"{string.Join(", ", rollExpressions)}");
                    }
                }

                // Apply the "o" adjustment to the total
                if (!string.IsNullOrEmpty(increaseGroup2))
                {
                    int increaseValue2 = int.Parse(increaseGroup2);
                    total += increaseValue2;
                }

                // Apply the "g" adjustment to the total
                int groupModifier = string.IsNullOrEmpty(increaseGroup3) ? 0 : int.Parse(increaseGroup3);
                total += groupModifier;

                // Append the total to the output
                if (includeExpression)
                {
                    if (!string.IsNullOrEmpty(increaseGroup2))
                    {
                        sb.AppendLine($"That has an overall modifier of {increaseGroup2}");
                    }
                    if (!string.IsNullOrEmpty(increaseGroup3))
                    {
                        sb.AppendLine($"And a per-group modifier of {increaseGroup3} for a total of {total}");
                    }
                    else
                    {
                        sb.AppendLine($"For a total of {total}");
                    }
                }
                else
                {
                    sb.AppendLine($"{total}");
                }
            }

            // Return the result string
            return sb.ToString().TrimEnd();
        }
        else
        {
            // Handle the case where the option does not match the pattern
            throw new FormatException($"Invalid option format. option: {option}");
        }
    }
}
