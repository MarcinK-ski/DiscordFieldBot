using System;
using System.Collections.Generic;

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

        private List<Marks> _marks = new List<Marks>();
        public List<Marks> GetMarksList()
            => _marks;

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
        public float GetLastMarksAvg()
        { 
            if (_marks.Count > 0)
            {
                if (_MarksWasChanged)
                {
                    _MarksWasChanged = false;

                    float sum = 0;
                    foreach(Marks mark in _marks)
                    {
                        sum += mark.Grade * mark.Weight;
                    }

                    _lastMarksAvg = sum / _marks.Count;
                }
            }
            else
            {
                return 0;
            }

            return _lastMarksAvg;
        }

        public int Get100ProcentOfMarks()
        {
            if (_marks.Count < 1)
            {
                return 0;
            }

            int toReturn = 0;
            foreach (var item in _marks)
            {
                toReturn += item.Weight * Marks.MAX_MARK;
            }

            return toReturn / _marks.Count;
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
    }
}
