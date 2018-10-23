using System.Diagnostics;
using System.Threading.Tasks;

namespace DiscordFieldBot
{
    public class BashTools
    {
        public BashTools() { }

        public Process BashProcess { get; private set; } = null;
        public void SetBashProcess(string cmd)
        {
            KillBashProcess();

            Process process = _PrepareProcess(cmd);

            process.Start();
            BashProcess = process;
        }

        public void KillBashProcess()
        {
            if (BashProcess != null)
            {
                if (!BashProcess.HasExited)
                {
                    BashProcess.Kill();
                }
            }
        }

        public static async Task<string> BashCommandWithResult(string cmd)
        {
            Process process = _PrepareProcess(cmd);

            process.Start();
            string result = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            return result;
        }

        private static Process _PrepareProcess(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            return new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
        }
    }
}
