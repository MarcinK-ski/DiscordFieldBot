using Discord.WebSocket;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordFieldBot
{
    class CamBot
    {
        private static readonly ulong _readOnlyChannel = ulong.Parse(Helper.channels["Cam"]);

        private static SshTools _ssh = new SshTools(Helper.sshSettings["User"], Helper.sshSettings["Ip"]);

        private static readonly string _camIp = Helper.camSettings["Ip"];
        private static readonly string _port = Helper.camSettings["Port"];

        public static async Task CamBotCommandsAsync(string command, SocketMessage message)
        {
            var channel = message.Channel;

            if (channel.Id == _readOnlyChannel)
            {
                if (new Regex("(po[l|ł][a|ą]cz)|p$").IsMatch(command))
                {
                    await TryConnect(channel);
                }
                else if (new Regex("(roz[l|ł][a|ą]cz)|r$").IsMatch(command))
                {
                    await TryDisconnect(channel);
                }
                else if (new Regex("(sprawd[z|ź])|s$").IsMatch(command))
                {
                    await channel.SendMessageAsync(await _ssh.CheckConnectionType(_port));
                }
                else if ("pomoc" == command)
                {
                    await ShowHelp(channel);
                }
            }
            else
            {
                return;
            }
        }

        private static async Task ShowHelp(ISocketMessageChannel channel)
        {
            await channel.SendMessageAsync
                (
                    $"Każda komenda musi zaczynać się znakiem: `{Bots.GetBotTypeChar(Bots.BotType.CamBot)}`" +
                    $"\nLista komend: " +
                    $"\n    - `polacz` lub `p` - umożliwia łączenie się z rejestratorem, " +
                    $"\n    - `rozlacz` lub `r` - zamyka połączenie z rejestratorem," +
                    $"\n    - `sprawdz` lub `s` - sprawdza, czy jest aktywne połączenie z rejestratorem," +
                    $"\n    - `pomoc` - wyświetla pomoc, czyli tę listę komend."
                );
        }

        private static async Task TryDisconnect(ISocketMessageChannel channel)
        {
            await _ssh.CheckConnectionType(_port);

            if (_ssh.Connected)
            {
                _ssh.DestorySshTunnel();
                await channel.SendMessageAsync("Rozłączono!");
            }
            else
            {
                await channel.SendMessageAsync("Połączenie nie jest nawiązane.");
            }
        }

        private static async Task TryConnect(ISocketMessageChannel channel)
        {
            await _ssh.CheckConnectionType(_port);

            if (_ssh.Connected)
            {
                await channel.SendMessageAsync("Połączenie już istnieje!");
            }
            else
            {
                await channel.SendMessageAsync("Próba połączenia...");

                try
                {
                    bool connectionSuccesful = await _ssh.CreateSshTunnel(_port, _port, _camIp);

                    if (connectionSuccesful)
                        await channel.SendMessageAsync("Połączono!");
                    else
                        await channel.SendMessageAsync("Nie udało się połączyć :(");
                }
                catch
                {
                    await channel.SendMessageAsync("Wystąpił wyjątak podczas łączenia się! Brak połączenia...");
                }
            }
        }
    }
}
