using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Calculator
{
    public abstract class CalcItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Id
        {
            get; set;
        }
        public DateTime Date
        {
            get; set;
        }
    }
    public class JournalCalcItem : CalcItem
    {
        public JournalCalcItem(int id, string expression, string result)
        {
            Id = id;
            Expression = expression;
            Result = result;
            Date = DateTime.Now;
        }

        public string Expression
        {
            get; set;
        }
        public string Result
        {
            get; set;
        }
    }
    public class MemoryCalcItem : CalcItem
    {
        public MemoryCalcItem(int id, string number)
        {
            Id = id;
            Date = DateTime.Now;
            Number = number;
        }

        private string _number;
        public string Number
        {
            get => _number;
            set
            {
                _number = value;
                OnPropertyChanged(nameof(Number));
            }
        }
    }
}
