﻿using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.IO;

namespace DiscordFieldBot
{
    public class DiscordConnection
    {
        private readonly string _token = Helper.config["Token"];

        public static DiscordSocketClient socketClient;

        static ConnectionState _lastConnectionState = ConnectionState.Disconnected;
        static DateTime _lastConnectionStateTS = DateTime.Now;

        public async Task MainAsync()
        {
            socketClient = new DiscordSocketClient();

            await Connect();

            Console.WriteLine("\nAll prefixes: ");
            for (int i = 0; i < Bots.botsPrefixes.Count; i++)
            {
                Console.WriteLine($"{(Bots.BotType)i}: \"{Bots.botsPrefixes[i]}\"");
            }
            Console.WriteLine('\n');

            var watchdogTimer = new System.Threading.Timer(async (e) =>
            {
                await UpdateWatchdogFile();
            }
            , null, TimeSpan.Zero, TimeSpan.FromMinutes(2));

            await Task.Delay(-1);
        }

        private async Task UpdateWatchdogFile()
        {
            if(_lastConnectionState != socketClient.ConnectionState)
            {
                _lastConnectionState = socketClient.ConnectionState;
                _lastConnectionStateTS = DateTime.Now;

                using (StreamWriter writer = new StreamWriter("DFBWatch"))
                {
                    await writer.WriteLineAsync($"{_lastConnectionStateTS} {_lastConnectionState}");
                }
            }
        }

        private async Task Connect()
        {
            AddEventsMethods();

            await socketClient.LoginAsync(TokenType.Bot, _token);
            await socketClient.StartAsync();
        }

        private void AddEventsMethods()
        {
            socketClient.MessageReceived += MessageHandler;
            socketClient.Log += Logging;
            socketClient.Ready += ClientReady;
        }

        private async Task MessageHandler(SocketMessage message)
        {
            char messagePrefix = message.Content[0];

            if (Bots.botsPrefixes.Exists(s => s == messagePrefix))
            {
                Console.WriteLine($"{DateTime.Now} -- Command: {message.Content} from user {message.Author.Username}#{message.Author.Id}");

                Bots.BotType botType = Bots.GetBotType(messagePrefix);

                string command = message.Content.Substring(1).ToLower().Trim();

                try
                {
                    switch (botType)
                    {
                        case Bots.BotType.CamBot:
                            await CamBot.CamBotCommandsAsync(command, message);
                            break;
                        case Bots.BotType.CleanerBot:
                            await CleanerBot.DeleteLastMessages(command, message);
                            break;
                        case Bots.BotType.MusicBot:
                            break;
                        case Bots.BotType.ChanceCalculateBot:
                            await CalculateChanceBot.HandleComandAsync(command, message);
                            break;
                        case Bots.BotType.Unknown:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Helper.ConsoleWriteExceptionInfo(ex);
                }
            }
        }

        private Task Logging(LogMessage arg)
        {
            Console.WriteLine($"{DateTime.Now} \n--- {arg.Message} \n");
            return Task.CompletedTask;
        }

        private Task ClientReady()
        {
            CalculateChanceBot.DeserializeDictionary();
            return Task.CompletedTask;
        }
    }
}
