using System;
using System.Collections.Generic;

namespace DiscordFieldBot
{
    public class Bots
    {
        private static readonly int _OFFSET = int.Parse(Helper.config["Offset"]);

        public static List<char> botsPrefixes = GetAllBotsPrefixes();


        public enum BotType
        {
            CamBot,
            CleanerBot,
            MusicBot,
            ChanceCalculateBot,
            Unknown
        }

        public static BotType GetBotType(char prefix)
        {
            switch (prefix - _OFFSET)
            {
                case (int)BotType.CamBot:
                    return BotType.CamBot;

                case (int)BotType.CleanerBot:
                    return BotType.CleanerBot;

                case (int)BotType.MusicBot:
                    return BotType.MusicBot;

                case (int)BotType.ChanceCalculateBot:
                    return BotType.ChanceCalculateBot;

                default:
                    return BotType.Unknown;
            }
        }

        public static char GetBotTypeChar(BotType botType)
            => (char)((int)botType + _OFFSET);

        private static List<char> GetAllBotsPrefixes()
        {
            List<char> prefixes = new List<char>();

            foreach (BotType item in Enum.GetValues(typeof(BotType)))
            {
                prefixes.Add(GetBotTypeChar(item));
            }

            return prefixes;
        }
    }
}
