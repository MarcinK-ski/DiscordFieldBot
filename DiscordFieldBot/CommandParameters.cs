using System.Collections.Generic;

namespace DiscordFieldBot
{
    class CommandParameters
    {
        public string Command { get; set; } = null;
        public List<string> Parameters { get; private set; } = new List<string>();
        public int CountParameters { get; private set; } = -1;
        private void SetCountParameters()
        {
            if(!string.IsNullOrWhiteSpace(Command))
            {
                CountParameters = Parameters.Count;
            }
            else
            {
                CountParameters = -1;
            }
        }

        public CommandParameters(params string[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if(i == 0)
                {
                    Command = parameters[i];
                    continue;
                }

                this.Parameters.Add(parameters[i]);
            }

            SetCountParameters();
        }
    }
}
