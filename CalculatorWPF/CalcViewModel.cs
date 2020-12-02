using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Calculator
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class CalcViewModel : BaseViewModel, IDataErrorInfo
    {
        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;

                switch (columnName)
                {
                    case nameof(Result):
                        if (!double.TryParse(Result, out _))
                        {
                            error = $"\"{Result}\" is not a number. May return unexpected result";
                        }
                        break;
                }

                return error;
            }
        }
        public string Error => null;

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
        public CalcCollectionDb<JournalCalcItem> Journal
        {
            get;
            set;
        }
        public CalcCollectionDb<MemoryCalcItem> Memory
        {
            get;
            set;
        }

        public CalcViewModel()
        {
            Result = "0";
            Expression = string.Empty;
            LastOperation = "=";

            #region Journal
            Journal = new CalcCollectionDb<JournalCalcItem>("Calculator", "Journal");
            if (Journal.TryLoad())
            {
                Journal.Collection = Journal.Load();
            }
            #endregion

            #region Memory
            Memory = new CalcCollectionDb<MemoryCalcItem>("Calculator", "Memory");
            if (Memory.TryLoad())
            {
                Memory.Collection = Memory.Load();
            }
            #endregion
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

            LastOperation = "Number";
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
            LastOperation = "Number";
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
            if (LastOperation == "Number" || LastOperation == "=")
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

                Journal.Add(new JournalCalcItem(Expression, Result));
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
            Result = CalcModel.Invert(Result);
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
                return _memoryRecallLastButtonPressCommand ??= new RelayCommand(MemoryRecallLastButtonPress, () => Memory.Collection.Count != 0);
            }
        }
        public void MemoryRecallLastButtonPress()
        {
            if (LastOperation == "=")
            {
                Expression = string.Empty;
            }
            if (Memory.Collection.Count != 0)
            {
                Result = Memory.Collection[0].Number;
                LastOperation = "Number";
            }
        }

        //  Добавить к значению на дисплее последний добавленный элемент в память (Result += Memory[0])
        private ICommand _memoryAddLastButtonPressCommand;
        public ICommand MemoryAddLastButtonPressCommand
        {
            get
            {
                return _memoryAddLastButtonPressCommand ??= new RelayCommand(MemoryAddLastButtonPress, () => Memory.Collection.Count != 0);
            }
        }
        public void MemoryAddLastButtonPress()
        {
            if (LastOperation == "=")
            {
                Expression = string.Empty;
            }
            if (Memory.Collection.Count != 0)
            {
                Result = CalcModel.Calculate($"{Result} + {Memory.Collection[0].Number}");
            }
        }

        //  Вычесть из значения на дисплее последний добавленный элемент в память (Result -= Memory[0])
        private ICommand _memorySubtractLastButtonPressCommand;
        public ICommand MemorySubtractLastButtonPressCommand
        {
            get
            {
                return _memorySubtractLastButtonPressCommand ??= new RelayCommand(MemorySubtractLastButtonPress, () => Memory.Collection.Count != 0);
            }
        }
        public void MemorySubtractLastButtonPress()
        {
            if (LastOperation == "=")
            {
                Expression = string.Empty;
            }
            if (Memory.Collection.Count != 0)
            {
                Result = CalcModel.Calculate($"{Result} - {Memory.Collection[0].Number}");
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
            Memory.Add(new MemoryCalcItem(Result));
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
        }


        //  Извлечь значение из памяти
        private ICommand _memoryRecallButtonPressCommand;
        public ICommand MemoryRecallButtonPressCommand
        {
            get
            {
                return _memoryRecallButtonPressCommand ??= new RelayCommand<MemoryCalcItem>(MemoryRecallButtonPress, memoryItem => true);
            }
        }
        public void MemoryRecallButtonPress(MemoryCalcItem memoryItem)
        {
            if (LastOperation == "=")
            {
                Expression = string.Empty;
            }
            Result = memoryItem.Number;
            LastOperation = "Number";
        }

        //  Удалить элемент из памяти
        private ICommand _memoryClearButtonPressCommand;
        public ICommand MemoryClearButtonPressCommand
        {
            get
            {
                return _memoryClearButtonPressCommand ??= new RelayCommand<MemoryCalcItem>(MemoryClearButtonPress, memoryItem => true);
            }
        }
        public void MemoryClearButtonPress(MemoryCalcItem memoryItem)
        {
            Memory.Remove(memoryItem);
        }

        //  Добавить число на дисплее к ячейке памяти (Memory[index] += Result)
        private ICommand _memoryAddButtonPressCommand;
        public ICommand MemoryAddButtonPressCommand
        {
            get
            {
                return _memoryAddButtonPressCommand ??= new RelayCommand<MemoryCalcItem>(MemoryAddButtonPress, memoryItem => true);
            }
        }
        public void MemoryAddButtonPress(MemoryCalcItem memoryItem)
        {
            Memory.ChangeValue(memoryItem, "Number", CalcModel.Calculate($"{memoryItem.Number} + {Result}"));
        }

        //  Вычесть число на дисплее из ячейки памяти (Memory[index] -= Result)
        private ICommand _memorySubtractButtonPressCommand;
        public ICommand MemorySubtractButtonPressCommand
        {
            get
            {
                return _memorySubtractButtonPressCommand ??= new RelayCommand<MemoryCalcItem>(MemorySubtractButtonPress, memoryItem => true);
            }
        }
        public void MemorySubtractButtonPress(MemoryCalcItem memoryItem)
        {
            Memory.ChangeValue(memoryItem, "Number", CalcModel.Calculate($"{memoryItem.Number} - {Result}"));
        }
        #endregion
    }
}