using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;

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
            get;
            set;
        }
        public DateTime Date
        {
            get; set;
        }

        public abstract string ToText();
        public abstract void FromText(string text);
    }
    public class JournalCalcItem : CalcItem
    {
        public JournalCalcItem()
        {
            Id = 0;
            Date = DateTime.MinValue;
            Expression = string.Empty;
            Result = string.Empty;
        }
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

        public override string ToText()
        {
            return $"{Date}\x1F{Id}\x1F{Expression}\x1F{Result}";
        }
        public override void FromText(string text)
        {
            string[] textSplit = text.Split("\x1F");

            Date = DateTime.Parse(textSplit[0]);
            Id = int.Parse(textSplit[1]);
            Expression = textSplit[2];
            Result = textSplit[3];
        }
    }
    public class MemoryCalcItem : CalcItem
    {
        public MemoryCalcItem()
        {
            Id = 0;
            Date = DateTime.MinValue;
            Number = string.Empty;
        }
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

        public override string ToText()
        {
            return $"{Date}\x1F{Id}\x1F{Number}";
        }
        public override void FromText(string text)
        {
            string[] textSplit = text.Split("\x1F");

            Date = DateTime.Parse(textSplit[0]);
            Id = int.Parse(textSplit[1]);
            Number = textSplit[2];
        }
    }


    public interface ICalcCollection<T> where T : CalcItem
    {
        public ObservableCollection<T> Collection { get; set; }
        public void Save();
        public bool TryLoad();
        public ObservableCollection<T> Load();
    }


    public class CalcCollectionTxt<T> : ICalcCollection<T> where T : CalcItem, new()
    {
        private string FileName { get; }
        public ObservableCollection<T> Collection { get; set; }
        public CalcCollectionTxt(string fileName)
        {
            FileName = fileName;
            Collection = new ObservableCollection<T>();
        }

        public void Save()
        {
            string text = string.Empty;
            foreach (var calcItem in Collection)
            {
                text += $"{calcItem.ToText()}\n";
            }

            File.WriteAllText(FileName, text);
        }
        public bool TryLoad()
        {
            try
            {
                _ = File.ReadAllText(FileName);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public ObservableCollection<T> Load()
        {
            string[] text = File.ReadAllText(FileName).Split('\n');
            ObservableCollection<T> collection = new ObservableCollection<T>();

            foreach (var calcItemTxt in text)
            {
                if (calcItemTxt != string.Empty)
                {
                    var calcItem = new T();
                    calcItem.FromText(calcItemTxt);
                    collection.Add(calcItem);
                }
            }
            return collection;
        }
    }
    public class CalcCollectionJson<T> : ICalcCollection<T> where T : CalcItem
    {
        private string FileName
        {
            get;
        }
        public ObservableCollection<T> Collection
        {
            get;
            set;
        }
        public CalcCollectionJson(string fileName)
        {
            FileName = fileName;
            Collection = new ObservableCollection<T>();
        }

        public void Save()
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var json = JsonSerializer.Serialize(Collection, jsonOptions);
            File.WriteAllText(FileName, json);
        }
        public bool TryLoad()
        {
            try
            {
                var json = File.ReadAllText(FileName);
                JsonSerializer.Deserialize<ObservableCollection<T>>(json);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public ObservableCollection<T> Load()
        {
            var json = File.ReadAllText(FileName);
            var collection = JsonSerializer.Deserialize<ObservableCollection<T>>(json);
            for (int i = 0; i < collection?.Count; i++)
            {
                collection[i].Id = collection.Count - i;
            }

            return collection;
        }
    }
    public class CalcCollectionDb<T> : ICalcCollection<T> where T : CalcItem
    {
        private string FileName
        {
            get;
        }
        public ObservableCollection<T> Collection
        {
            get; set;
        }
        public CalcCollectionDb(string fileName)
        {
            FileName = fileName;
            Collection = new ObservableCollection<T>();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
        public bool TryLoad()
        {
            throw new NotImplementedException();
        }
        public ObservableCollection<T> Load()
        {
            throw new NotImplementedException();
        }
    }
}
