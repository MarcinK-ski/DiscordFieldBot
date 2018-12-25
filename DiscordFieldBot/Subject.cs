using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace DiscordFieldBot
{
    [Serializable]
    class Subject
    {
        public Subject(string subject, sbyte weight)
        {
            SubjectName = subject;
            SubjectWeight = weight;
        }
        
        public string SubjectName { get; set; }

        private bool _MarksWasChanged { get; set; } = true;

        [OptionalField]
        private Dictionary<Term, List<Marks>> _oldMarks = new Dictionary<Term, List<Marks>>();
        private List<Marks> _marks = new List<Marks>();
        public List<Marks> GetMarksList()
            => _marks;

        public override string ToString()
        {
            return $"{SubjectName} {SubjectWeight}";
        }

        private sbyte _subjectWeight;
        public sbyte SubjectWeight
        {
            get => _subjectWeight;

            set
            {
                if (value > 10)
                {
                    _subjectWeight = 10;
                }
                else if (value < 1)
                {
                    _subjectWeight = 1;
                }
                else
                {
                    _subjectWeight = value;
                }
            }
        } 

        private float _lastMarksAvg = 0F;
        public float GetLastMarksAvg(bool updateAvg = false)
        { 
            if (_marks.Count > 0)
            {
                if (_MarksWasChanged || updateAvg)
                {
                    _MarksWasChanged = false;

                    _lastMarksAvg = CalculateAgv(_marks);
                }
            }
            else
            {
                return 0;
            }

            return _lastMarksAvg;
        }

        private float CalculateAgv(List<Marks> marks)
        {
            float sum = 0;
            int marksCounter = 0;
            foreach (Marks mark in marks)
            {
                sum += mark.Grade * mark.Weight;
                marksCounter += mark.Weight;
            }

            return sum / marksCounter;
        }

        public int Get100ProcentOfMarks()
        {
            if (_marks.Count < 1)
            {
                return 0;
            }

            int toReturn = 0;
            int marksCounter = 0;
            foreach (var item in _marks)
            {
                toReturn += item.Weight * Marks.MAX_MARK;
                marksCounter += item.Weight;
            }

            return toReturn / marksCounter;
        }

        public void AddNewMark(float grade, sbyte weigth)
        {
            _marks.Add(new Marks(grade, weigth));
            _MarksWasChanged = true;
        }

        public void DeleteMark(float grade, sbyte weigth)
        {
            for (int i = 0; i < _marks.Count; i++)
            {
                if (_marks[i].Grade == grade && _marks[i].Weight == weigth)
                {
                    _marks.Remove(_marks[i]);
                    break;
                }
            }

            _MarksWasChanged = true;
        }

        public void ModifyMark(float oldGrade, float newGrade, sbyte oldWeigth, sbyte newWeight)
        {
            for (int i = 0; i < _marks.Count; i++)
            {
                if (_marks[i].Grade == oldGrade && _marks[i].Weight == oldWeigth)
                {
                    _marks.Insert(i, new Marks(newGrade, newWeight));
                    break;
                }
            }

            _MarksWasChanged = true;
        }

        public string ShowSubjectDetails(bool skipNoMarks = false)
        {
            if (GetMarksList().Count < 1)
            {
                if (skipNoMarks)
                {
                    return "";
                }
                else
                {
                    return $"Brak ocen z przedmiotu `{SubjectName}`\n";
                }
            }
            else
            {
                string resultToSend = $"Oceny z przedmiotu: `{SubjectName}`\n";
                foreach (var item in GetMarksList())
                {
                    resultToSend += $"*  `{item.Grade}` wagą: {item.Weight}\n";
                }

                return $"{resultToSend}Średnia: *{GetLastMarksAvg()}*\n\n";
            }
        }


        private string ShowSubjectPastDetails(List<Marks> marks, Term term)
        {
            if (marks.Count < 1)
            {
                return "";
            }
            else
            {
                string resultToSend = $"Oceny z przedmiotu: `{SubjectName}` {term}\n";
                foreach (var item in marks)
                {
                    resultToSend += $"*  `{item.Grade}` waga: {item.Weight}\n";
                }

                float termAvg = term.TermAvg;
                if (termAvg == 0)
                {
                    termAvg = term.TermAvg = CalculateAgv(marks);
                }

                return $"{resultToSend}Średnia: *{termAvg}*\n\n";
            }
        }

        public void ArchivizeMarks(int yearNo, int termNo)
        {
            Term term = new Term(yearNo, termNo, GetLastMarksAvg(true));
            
            if(_oldMarks == null)   //For app upgrade from previous version of binary
            {
                _oldMarks = new Dictionary<Term, List<Marks>>();
            }

            _oldMarks.Add(term, new List<Marks>(_marks));
            _marks.Clear();
            _lastMarksAvg = 0F;
        }

        public bool IsTermArchived(Term term)
        {
            if(_oldMarks.ContainsKey(term))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetAllTerms()
        {
            StringBuilder toReturn = new StringBuilder();

            foreach (var term in _oldMarks.Keys)
            {
                toReturn.AppendLine($"*{term.ToString()}*");
            }

            return toReturn.ToString();
        }

        public string GetArchivizedMarks()
        {
            StringBuilder toReturn = new StringBuilder("");
            foreach (var item in _oldMarks)
            {
                Term term = item.Key;
                toReturn.Append(GetArchivizedMarks(term));
            }

            return toReturn.ToString();
        }

        public string GetArchivizedMarks(Term term)
        {
            foreach (var marks in _oldMarks)
            {
                if(marks.Key.Equals(term))
                {
                    return ShowSubjectPastDetails(marks.Value, marks.Key);
                }
            }

            return "";
        }
    }
}
