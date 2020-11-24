using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;


namespace Calculator
{
    public abstract class BaseViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string this[string columnName]
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public string Error
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
    public class CalcViewModel : BaseViewModel
    {
        public CalcViewModel()
        {
            _result = "0";
            _expression = string.Empty;
            LastOperation = "=";
            try
            {
                Journal = JsonSerializer.Deserialize<ObservableCollection<JournalCalcItem>>(File.ReadAllText("Journal.json"));
            }
            catch
            {
                Journal = new ObservableCollection<JournalCalcItem>();
            }
            try
            {
                Memory = JsonSerializer.Deserialize<ObservableCollection<MemoryCalcItem>>(File.ReadAllText("Memory.json"));
            }
            catch 
            {
                Memory = new ObservableCollection<MemoryCalcItem>();
            }
        }

        private string _result;
        public string Result
        {
            get => _result;
            set
            {
                _result = value;
                OnPropertyChanged(nameof(Result));
            }
        }
        private string _expression;
        public string Expression
        {
            get => _expression;
            set
            {
                _expression = value;
                OnPropertyChanged(nameof(Expression));
            }
        }
        public string LastOperation
        {
            get;
            set;
        }
        public ObservableCollection<JournalCalcItem> Journal
        {
            get;
            set;
        }
        public ObservableCollection<MemoryCalcItem> Memory
        {
            get;
            set;
        }



        #region Numbers ICommands
        //  Цифры
        private ICommand _digitButtonPressCommand;
        public ICommand DigitButtonPressCommand
        {
            get
            {
                return _digitButtonPressCommand ??= new RelayCommand<string>(DigitButtonPress, digit => true);
            }
        }
        public void DigitButtonPress(string digit)
        {
            if (LastOperation == "=")
            {
                Result = "0";
                LastOperation = string.Empty;
                Expression = string.Empty;
            }
            if (Result == "0")
            {
                Result = digit;
            }
            else if (Result.Length < 16)
            {
                Result += digit;
            }

            LastOperation = "Id";
        }

        //  Запятая
        private ICommand _pointButtonPressCommand;
        public ICommand PointButtonPressCommand
        {
            get
            {
                return _pointButtonPressCommand ??= new RelayCommand(PointButtonPress);
            }
        }
        public void PointButtonPress()
        {
            if (LastOperation == "=")
            {
                Result = "0";
                LastOperation = string.Empty;
                Expression = string.Empty;
            }
            if (!Result.Contains(","))
            {
                Result += ",";
            }
            LastOperation = "Id";
        }
        #endregion

        #region Operations ICommands
        //  Арифметические операции
        private ICommand _arithmeticOperationButtonPress;
        public ICommand ArithmeticOperationButtonPressCommand
        {
            get
            {
                return _arithmeticOperationButtonPress ??= new RelayCommand<string>(ArithmeticOperationButtonPress, operation => true);
            }
        }
        public void ArithmeticOperationButtonPress(string operation)
        {
            if (Result[^1] == ',')
            {
                Result = Result.Remove(Result.Length - 1);
            }
            if (LastOperation == "=")
            {
                Expression = string.Empty;
            }
            if (LastOperation == "Id" || LastOperation == "=")
            {
                Expression += $"{Result} {operation} ";
                Result = "0";
            }
            else
            {
                Expression = Expression.Remove(Expression.Length - 2);
                Expression += $"{operation} ";
            }

            LastOperation = operation;
        }

        //  Вычисление
        private ICommand _equalButtonPressCommand;
        public ICommand EqualButtonPressCommand
        {
            get
            {
                return _equalButtonPressCommand ??= new RelayCommand(EqualButtonPress);
            }
        }
        public void EqualButtonPress()
        {
            if (Result[^1] == ',')
            {
                Result = Result.Remove(Result.Length - 1);
            }
            if (LastOperation == "=")
            {
                Expression = $"{Result} =";
            }
            else
            {
                Expression += $"{Result} ";
                Result = CalcModel.Calculate(Expression);
                Expression += "=";
                Journal.Insert(0, new JournalCalcItem(Journal.Count + 1, Expression, Result));

                File.WriteAllText("Journal.json", JsonSerializer.Serialize(Journal, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
            }
            LastOperation = "=";
        }

        //  Процент
        private ICommand _percentButtonPressCommand;
        public ICommand PercentButtonPressCommand
        {
            get
            {
                return _percentButtonPressCommand ??= new RelayCommand(PercentButtonPress);
            }
        }
        public void PercentButtonPress()
        {
            if (Result[^1] == ',')
            {
                Result = Result.Remove(Result.Length - 1);
            }
            Result = CalcModel.Percent(Expression, Result);
        }

        //  Инверсия значения
        private ICommand _invertButtonPressCommand;
        public ICommand InvertButtonPressCommand
        {
            get
            {
                return _invertButtonPressCommand ??= new RelayCommand(InvertButtonPress);
            }
        }
        public void InvertButtonPress()
        {
            if (Result[^1] == ',')
            {
                Result = Result.Remove(Result.Length - 1);
            }
            if (double.Parse(Result) != 0)
            {
                Result = CalcModel.Invert(Result);
            }
        }
        #endregion

        #region App actions ICommands
        //  Очистка экранов (Result)
        private ICommand _clearButtonPressCommand;
        public ICommand ClearButtonPressCommand
        {
            get
            {
                return _clearButtonPressCommand ??= new RelayCommand(ClearButtonPress);
            }
        }
        public void ClearButtonPress()
        {
            if (LastOperation == "=" || Result == "0")
            {
                Result = "0";
                Expression = string.Empty;
            }
            else
            {
                Result = "0";
            }
        }

        //  Показ окна (UIElement)
        private ICommand _showWindowButtonPressCommand;
        public ICommand ShowWindowButtonPressCommand
        {
            get
            {
                return _showWindowButtonPressCommand ??= new RelayCommand<UIElement>(ShowWindowButtonPress, grid => true);
            }
        }
        public static void ShowWindowButtonPress(UIElement uiElement)
        {
            uiElement.Visibility = uiElement.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion

        #region Journal ICommands
        //  Очистка журнала
        private ICommand _journalClearButtonPressCommand;
        public ICommand JournalClearButtonPressCommand
        {
            get
            {
                return _journalClearButtonPressCommand ??= new RelayCommand(JournalClearButtonPress);
            }
        }
        public void JournalClearButtonPress()
        {
            Journal.Clear();

            File.WriteAllText("Journal.json", "");
        }

        private ICommand _journalRecallButtonPressCommand;
        public ICommand JournalRecallButtonPressCommand
        {
            get
            {
                return _journalRecallButtonPressCommand ??= new RelayCommand<JournalCalcItem>(JournalRecallButtonPress, journalItem => true);
            }
        }
        public void JournalRecallButtonPress(JournalCalcItem journalItem)
        {
            Result = journalItem.Result;
            Expression = journalItem.Expression;
            LastOperation = "=";
        }
        #endregion

        #region Memory ICommands
        //  Извлечь последнее добавленное значение из памяти
        private ICommand _memoryRecallLastButtonPressCommand;
        public ICommand MemoryRecallLastButtonPressCommand
        {
            get
            {
                return _memoryRecallLastButtonPressCommand ??= new RelayCommand(MemoryRecallLastButtonPress);
            }
        }
        public void MemoryRecallLastButtonPress()
        {
            if (LastOperation == "=")
            {
                Expression = string.Empty;
            }
            if (Memory.Count != 0)
            {
                Result = Memory[0].Number;
                LastOperation = "Id";
            }
        }

        //  Добавить к значению на дисплее последний добавленный элемент в память (Result += Memory[0])
        private ICommand _memoryAddLastButtonPressCommand;
        public ICommand MemoryAddLastButtonPressCommand
        {
            get
            {
                return _memoryAddLastButtonPressCommand ??= new RelayCommand(MemoryAddLastButtonPress);
            }
        }
        public void MemoryAddLastButtonPress()
        {
            if (Memory.Count != 0)
            {
                Result = CalcModel.Calculate($"{Result} + {Memory[0].Number}");
            }
        }

        //  Вычесть из значения на дисплее последний добавленный элемент в память (Result -= Memory[0])
        private ICommand _memorySubtractLastButtonPressCommand;
        public ICommand MemorySubtractLastButtonPressCommand
        {
            get
            {
                return _memorySubtractLastButtonPressCommand ??= new RelayCommand(MemorySubtractLastButtonPress);
            }
        }
        public void MemorySubtractLastButtonPress()
        {
            if (Memory.Count != 0)
            {
                Result = CalcModel.Calculate($"{Result} - {Memory[0].Number}");
            }
        }

        //  Сохранить значение в память
        private ICommand _memorySaveButtonPressCommand;
        public ICommand MemorySaveButtonPressCommand
        {
            get
            {
                return _memorySaveButtonPressCommand ??= new RelayCommand(MemorySaveButtonPress);
            }
        }
        public void MemorySaveButtonPress()
        {
            Memory.Insert(0, new MemoryCalcItem(Memory.Count + 1, Result));

            File.WriteAllText("Memory.json", JsonSerializer.Serialize(Memory, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
        }

        //  Полная очистка памяти
        private ICommand _memoryClearAllButtonPressCommand;
        public ICommand MemoryClearAllButtonPressCommand
        {
            get
            {
                return _memoryClearAllButtonPressCommand ??= new RelayCommand(MemoryClearAllButtonPress);
            }
        }
        public void MemoryClearAllButtonPress()
        {
            Memory.Clear();

            File.WriteAllText("Memory.json", string.Empty);
        }


        //  Извлечь значение из памяти
        private ICommand _memoryRecallButtonPressCommand;
        public ICommand MemoryRecallButtonPressCommand
        {
            get
            {
                return _memoryRecallButtonPressCommand ??= new RelayCommand<int>(MemoryRecallButtonPress, index => true);
            }
        }
        public void MemoryRecallButtonPress(int index)
        {
            if (index != -1)
            {
                Result = Memory[index].Number;
                LastOperation = "Id";
            }
        }

        //  Удалить элемент из памяти
        private ICommand _memoryClearButtonPressCommand;
        public ICommand MemoryClearButtonPressCommand
        {
            get
            {
                return _memoryClearButtonPressCommand ??= new RelayCommand<int>(MemoryClearButtonPress, index => true);
            }
        }
        public void MemoryClearButtonPress(int index)
        {
            if (index != -1)
            {
                Memory.RemoveAt(index);
            }

            File.WriteAllText("Memory.json", JsonSerializer.Serialize(Memory, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
        }

        //  Добавить число на дисплее к ячейке памяти (Memory[index] += Result)
        private ICommand _memoryAddButtonPressCommand;
        public ICommand MemoryAddButtonPressCommand
        {
            get
            {
                return _memoryAddButtonPressCommand ??= new RelayCommand<int>(MemoryAddButtonPress, index => true);
            }
        }
        public void MemoryAddButtonPress(int index)
        {
            if (index != -1)
            {
                Memory[index].Number = CalcModel.Calculate($"{Memory[index].Number} + {Result}");

                File.WriteAllText("Memory.json", JsonSerializer.Serialize(Memory, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
            }
        }

        //  Вычесть число на дисплее из ячейки памяти (Memory[index] -= Result)
        private ICommand _memorySubtractButtonPressCommand;
        public ICommand MemorySubtractButtonPressCommand
        {
            get
            {
                return _memorySubtractButtonPressCommand ??= new RelayCommand<int>(MemorySubtractButtonPress, index => true);
            }
        }
        public void MemorySubtractButtonPress(int index)
        {
            if (index != -1)
            {
                Memory[index].Number = CalcModel.Calculate($"{Memory[index].Number} - {Result}");

                File.WriteAllText("Memory.json", JsonSerializer.Serialize(Memory, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
            }
        }
        #endregion

    }
}