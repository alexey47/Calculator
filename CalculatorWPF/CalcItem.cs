﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using Dapper;

namespace Calculator
{
    public abstract class CalcItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected CalcItem()
        {
            Id = "0";
            Date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
        }

        public string Id
        {
            get;
            set;
        }
        public string Date
        {
            get;
            set;
        }

        public abstract string ToText();
        public abstract void FromText(string text);
    }
    public class JournalCalcItem : CalcItem
    {
        public JournalCalcItem()
        {
            Expression = string.Empty;
            Result = string.Empty;
        }
        public JournalCalcItem(string expression, string result)
        {
            Expression = expression;
            Result = result;
        }

        public string Expression
        {
            get;
            set;
        }
        public string Result
        {
            get;
            set;
        }

        public override string ToText()
        {
            return $"{Date}\x1F{Id}\x1F{Expression}\x1F{Result}";
        }
        public override void FromText(string text)
        {
            string[] textSplit = text.Split("\x1F");

            Date = textSplit[0];
            Id = textSplit[1];
            Expression = textSplit[2];
            Result = textSplit[3];
        }
    }
    public class MemoryCalcItem : CalcItem
    {
        public MemoryCalcItem()
        {
            Number = string.Empty;
        }
        public MemoryCalcItem(string number)
        {
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

            Date = textSplit[0];
            Id = textSplit[1];
            Number = textSplit[2];
        }
    }

    public interface ICalcCollection<TCalcItem> where TCalcItem : CalcItem
    {
        public ObservableCollection<TCalcItem> Collection
        {
            get; set;
        }
        public bool TryLoad();
        public ObservableCollection<TCalcItem> Load();
        public void Add(TCalcItem item);
        public void Remove(TCalcItem item);
        public void Clear();
        public void ChangeValue(TCalcItem item, string propertyName, string newValue);
    }
    public class CalcCollectionTxt<TCalcItem> : ICalcCollection<TCalcItem> where TCalcItem : CalcItem, new()
    {
        private string FileName
        {
            get;
        }
        public ObservableCollection<TCalcItem> Collection
        {
            get;
            set;
        }

        public CalcCollectionTxt(string fileName)
        {
            FileName = fileName + ".txt";
            Collection = new ObservableCollection<TCalcItem>();
        }

        public bool TryLoad()
        {
            try
            {
                var text = File.ReadAllText(FileName).Split('\n');
                foreach (var calcItemTxt in text)
                {
                    if (calcItemTxt != string.Empty)
                    {
                        new TCalcItem().FromText(calcItemTxt);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public ObservableCollection<TCalcItem> Load()
        {
            string[] text = File.ReadAllText(FileName).Split('\n');
            ObservableCollection<TCalcItem> collection = new ObservableCollection<TCalcItem>();

            foreach (var calcItemTxt in text)
            {
                if (calcItemTxt != string.Empty)
                {
                    var calcItem = new TCalcItem();
                    calcItem.FromText(calcItemTxt);
                    collection.Add(calcItem);
                }
            }
            collection = new ObservableCollection<TCalcItem>(collection.OrderByDescending(item => item.Date));
            for (int i = 0; i < collection.Count; i++)
            {
                collection[i].Id = (collection.Count - i).ToString();
            }

            return collection;
        }
        private void Update()
        {
            string text = string.Empty;
            foreach (var calcItem in Collection)
            {
                text += $"{calcItem.ToText()}\n";
            }

            File.WriteAllText(FileName, text);
        }
        public void Add(TCalcItem item)
        {
            Collection.Insert(0, item);
            item.Id = Collection.Count.ToString();

            Update();
        }
        public void Remove(TCalcItem item)
        {
            Collection.Remove(item);
            for (int i = 0; i < Collection.Count; i++)
            {
                Collection[i].Id = (Collection.Count - i).ToString();
            }

            Update();
        }
        public void Clear()
        {
            Collection.Clear();

            Update();
        }
        public void ChangeValue(TCalcItem item, string propertyName, string newValue)
        {
            Collection[Collection.IndexOf(item)].GetType().GetProperty(propertyName)?.SetValue(item, newValue);

            Update();
        }
    }
    public class CalcCollectionJson<TCalcItem> : ICalcCollection<TCalcItem> where TCalcItem : CalcItem
    {
        private string FileName
        {
            get;
        }
        public ObservableCollection<TCalcItem> Collection
        {
            get;
            set;
        }

        public CalcCollectionJson(string fileName)
        {
            FileName = fileName + ".json";
            Collection = new ObservableCollection<TCalcItem>();
        }

        public bool TryLoad()
        {
            try
            {
                var json = File.ReadAllText(FileName);
                JsonSerializer.Deserialize<ObservableCollection<TCalcItem>>(json);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public ObservableCollection<TCalcItem> Load()
        {
            var json = File.ReadAllText(FileName);
            var collection = JsonSerializer.Deserialize<ObservableCollection<TCalcItem>>(json);
            collection = new ObservableCollection<TCalcItem>(collection!.OrderByDescending(item => item.Date));
            for (int i = 0; i < collection.Count; i++)
            {
                collection[i].Id = (collection.Count - i).ToString();
            }

            return collection;
        }
        private void Update()
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var json = JsonSerializer.Serialize(Collection, jsonOptions);

            File.WriteAllText(FileName, json);
        }
        public void Add(TCalcItem item)
        {
            Collection.Insert(0, item);
            item.Id = Collection.Count.ToString();

            Update();
        }
        public void Remove(TCalcItem item)
        {
            Collection.Remove(item);
            for (int i = 0; i < Collection.Count; i++)
            {
                Collection[i].Id = (Collection.Count - i).ToString();
            }

            Update();
        }
        public void Clear()
        {
            Collection.Clear();

            Update();
        }
        public void ChangeValue(TCalcItem item, string propertyName, string newValue)
        {
            Collection[Collection.IndexOf(item)].GetType().GetProperty(propertyName)?.SetValue(item, newValue);

            Update();
        }
    }
    public class CalcCollectionDb<TCalcItem> : ICalcCollection<TCalcItem> where TCalcItem : CalcItem
    {
        private string DataBaseName
        {
            get;
        }
        private string TableName
        {
            get;
        }
        public ObservableCollection<TCalcItem> Collection
        {
            get;
            set;
        }

        public CalcCollectionDb(string dataBaseName, string tableName)
        {
            DataBaseName = dataBaseName + ".db";
            TableName = tableName;
            Collection = new ObservableCollection<TCalcItem>();

            if (!File.Exists(DataBaseName))
            {
                SQLiteConnection.CreateFile(DataBaseName);
            }
            var properties = typeof(TCalcItem).GetProperties();
            using var dataBase = new SQLiteConnection($"Data Source={DataBaseName}; Version=3");
            dataBase.Open();

            string command = @$"Create table if not exists {TableName}(";
            for (int i = 0; i < properties.Length; i++)
            {
                command += $"{properties[i].Name} string";
                if (properties[i].Name == "Id")
                {
                    command += " Primary Key";
                }
                if (i < properties.Length - 1)
                {
                    command += ",";
                }
            }
            command += ")";

            dataBase.Query(command);
            dataBase.Close();
        }

        public bool TryLoad()
        {
            try
            {
                using var connection = new SQLiteConnection($"Data Source={DataBaseName}; Version=3");
                connection.Open();
                connection.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public ObservableCollection<TCalcItem> Load()
        {
            ObservableCollection<TCalcItem> collection = new ObservableCollection<TCalcItem>();
            using SQLiteConnection dataBase = new SQLiteConnection($"Data Source={DataBaseName}; Version=3");

            dataBase.Open();
            IEnumerable<TCalcItem> calcItems = dataBase.Query<TCalcItem>($"Select * from {TableName}");
            foreach (TCalcItem calcItem in calcItems)
            {
                collection.Insert(0, calcItem);
            }
            dataBase.Close();

            for (int i = 0; i < collection.Count; i++)
            {
                collection[i].Id = (collection.Count - i).ToString();
            }

            return collection;
        }
        public void Add(TCalcItem item)
        {
            Collection.Insert(0, item);
            item.Id = Collection.Count.ToString();

            using SQLiteConnection dataBase = new SQLiteConnection($"Data Source={DataBaseName}; Version=3");
            var properties = typeof(TCalcItem).GetProperties();
            dataBase.Open();

            //  command: Перечисление всех полей класса и запись в SQL запрос:
            //      "Insert into {TableName}(properties[i], properties[i + 1], ..., properties[n])
            //       Value($properties[i], $properties[i + 1], ..., properties[n],)";
            string command = @$"Insert into {TableName}(";
            for (int i = 0; i < properties.Length; i++)
            {
                command += $"{properties[i].Name}";
                if (i < properties.Length - 1)
                {
                    command += ",";
                }
            }
            command += ") Values(";
            var parameters = new DynamicParameters();
            for (int i = 0; i < properties.Length; i++)
            {
                command += $"${properties[i].Name}";
                if (i < properties.Length - 1)
                {
                    command += ",";
                }
                parameters.Add($"{properties[i].Name}", item.GetType().GetProperties()[i].GetValue(item));
            }
            command += ")";

            dataBase.Query<TCalcItem>(command, parameters);
            dataBase.Close();
        }
        public void Remove(TCalcItem item)
        {
            Collection.Remove(item);
            for (int i = 0; i < Collection.Count; i++)
            {
                Collection[i].Id = (Collection.Count - i).ToString();
            }

            using SQLiteConnection dataBase = new SQLiteConnection($"Data Source={DataBaseName}; Version=3");
            dataBase.Open();
            dataBase.Query(@$"Delete from {TableName}
                                  Where Id = {item.Id}");
            for (int i = int.Parse(item.Id) + 1; i <= Collection.Count + 1; i++)
            {
                dataBase.Query(@$"Update {TableName}
                                      Set Id = {i - 1} where Id = {i}");
            }
            dataBase.Close();
        }
        public void Clear()
        {
            Collection.Clear();

            using SQLiteConnection dataBase = new SQLiteConnection($"Data Source={DataBaseName}; Version=3");
            dataBase.Open();
            dataBase.Query($"Delete from {TableName}");
            dataBase.Close();
        }
        public void ChangeValue(TCalcItem item, string propertyName, string newValue)
        {
            Collection[Collection.IndexOf(item)].GetType().GetProperty(propertyName)?.SetValue(item, newValue);

            using SQLiteConnection dataBase = new SQLiteConnection($"Data Source={DataBaseName}; Version=3");
            dataBase.Open();
            dataBase.Query(@$"Update {TableName}
                                 Set {propertyName} = {newValue} where Id = {item.Id}");
            dataBase.Close();
        }
    }
}
