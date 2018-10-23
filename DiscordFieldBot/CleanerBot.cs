using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscordFieldBot
{
    class CleanerBot
    {
        private static readonly ulong _permitedUserId = 10;

        public static async Task DeleteLastMessages(string command, SocketMessage message)
        {
            if (Int32.TryParse(command, out int limitation))
            {
                if (message.Author.Id == _permitedUserId)
                {
                    var channel = message.Channel;

                    var messages = await channel.GetMessagesAsync(limitation + 1).Flatten();
                    await channel.DeleteMessagesAsync(messages);

                    var replay = await channel.SendMessageAsync($"Purged last {limitation} messages!");
                    await Task.Delay(3000);
                    await replay.DeleteAsync();
                }
            }
        }
    }
}
