using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordFieldBot
{
    [Serializable]
    class Term
    {
        public int YearNo { get; set; }
        public int TermNo { get; set; }
        public float TermAvg { get; set; }

        public Term(int yearNo, int term)
        {
            YearNo = yearNo;
            TermNo = term;
        }

        public Term(int yearNo, int term, float avg)
        {
            YearNo = yearNo;
            TermNo = term;
            TermAvg = avg;
        }

        public override string ToString()
        {
            return $"Klasa: {YearNo}, Semestr: {TermNo}";
        }

        public override int GetHashCode()
        {
            return Int32.Parse($"{YearNo}{TermNo}");
        }

        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == this.GetHashCode()? true : false;
        }
    }
}
