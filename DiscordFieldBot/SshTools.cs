using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordFieldBot
{
    public class SshTools
    {
        public string User { get; set; }
        public string Host { get; set; }
        public bool Connected { get; set; } = false;
        public string LastTunnelRemotePort { get; set; } = null;

        private BashTools _bash = new BashTools();

        public SshTools(string user, string host)
        {
            User = user;
            Host = host;
        }

        public async Task<bool> CreateSshTunnel(string localPort, string remotePort, string localIp, string fromAllowedIp = "0.0.0.0")
        {
            _bash.SetBashProcess($"ssh -Nv -R {fromAllowedIp}:{remotePort}:{localIp}:{localPort} {User}@{Host}");
            if (await CheckSshOnPortConnection(remotePort) == null)
            {
                return Connected = false;
            }

            return Connected = true;
        }

        public void DestorySshTunnel()
        {
            _bash.KillBashProcess();

            Connected = false;
        }

        public async Task<string> CheckSshOnPortConnection(string onPort)
        {
            if (!string.IsNullOrWhiteSpace(onPort))
            {
                string tekst = await BashTools.BashCommandWithResult($"ssh {User}@{Host} netstat -tapn");
                Regex regex = new Regex($@"[0-255].[0-255].[0-255].[0-255](?=:{onPort})");
                Match match = regex.Match(tekst);
                if (match.Success)
                    return match.Groups[0].Value;

                return null;
            }
            else
            {
                return null;
            }
        }

        public async Task<string> CheckConnectionType(string connectionForPort)
        {
            string connectionResult = await CheckSshOnPortConnection(connectionForPort);

            switch (connectionResult)
            {
                case "0.0.0.0":
                    Connected = true;
                    return "Połączenie zdalne";
                case "127.0.0.1":
                    Connected = true;
                    return "Połączenie lokalne";
                case null:
                    Connected = false;
                    return "Brak połączenia";
                default:
                    return $"Połączenie nieznane ({connectionResult})";
            }
        }
    }
}
