using System;

namespace DiscordFieldBot
{
    [Serializable]
    struct Marks
    {
        public sbyte Weight { get; private set; }
        public float Grade { get; private set; }

        public const int MAX_WEIGHT = 4;
        public const int MAX_MARK = 6;

        public Marks(float mark, sbyte weight)
        {
            if (weight > MAX_WEIGHT)
            {
                Weight = MAX_WEIGHT;
            }
            else if (weight < 1)
            {
                Weight = 1;
            }
            else
            {
                Weight = weight;
            }

            Grade = mark;
        }

        public override string ToString()
        {
            return $"Ocena: {Grade} Waga: {Weight}";
        }
    }
}
