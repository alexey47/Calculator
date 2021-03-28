using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Calculator
{
    public abstract class BaseViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public abstract string this[string columnName]
        {
            get;
        }
        public string Error => null;
    }
    public class CalcViewModel : BaseViewModel
    {
        public override string this[string columnName]
        {
            get
            {
                string error = string.Empty;

                switch (columnName)
                {
                    case nameof(Result):
                        if (!decimal.TryParse(Result, NumberStyles.Float, new CultureInfo("en-US"), out _))
                        {
                            error = $"\"{Result}\" is not a number. May return unexpected result";
                        }
                        break;
                }

                return error;
            }
        }

        public CalcViewModel()
        {
            Result = "0";
            Expression = string.Empty;
            _lastOperation = "=";

            #region Journal
            Journal = new CalcCollectionDb<JournalCalcItem>("Journal");
            if (Journal.TryLoad())
            {
                Journal.Collection = Journal.Load();
            }
            #endregion

            #region Memory
            Memory = new CalcCollectionDb<MemoryCalcItem>("Memory");
            if (Memory.TryLoad())
            {
                Memory.Collection = Memory.Load();
            }
            #endregion
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
        private string _lastOperation;
        private int _bracketsCount;
        public int BracketsCount
        {
            get => _bracketsCount;
            set
            {
                _bracketsCount = value;
                OnPropertyChanged(nameof(BracketsCount));
            }
        }
        public CalcCollection<JournalCalcItem> Journal
        {
            get;
            set;
        }
        public CalcCollection<MemoryCalcItem> Memory
        {
            get;
            set;
        }

        #region Numbers ICommands
        //  Цифры
        public ICommand DigitButtonPressCommand
        {
            get
            {
                return new RelayCommand<string>((digit) =>
                {
                    if (_lastOperation == "BracketClose")
                    {
                        int bracketOpenIndex = Expression.LastIndexOf('(');
                        int bracketCloseCounter = 0;
                        for (int i = bracketOpenIndex; i < Expression.Length; i++)
                        {
                            if (Expression[i] == ')')
                            {
                                bracketCloseCounter++;
                            }
                        }
                        BracketsCount += bracketCloseCounter - 1;

                        Expression = Expression.Remove(bracketOpenIndex);
                    }
                    if (_lastOperation == "=")
                    {
                        Result = "0";
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

                    _lastOperation = "Number";
                });
            }
        }
        //  Запятая
        public ICommand PointButtonPressCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (_lastOperation == "BracketClose")
                    {
                        int bracketOpenIndex = Expression.LastIndexOf('(');
                        int bracketCloseCounter = 0;
                        for (int i = bracketOpenIndex; i < Expression.Length; i++)
                        {
                            if (Expression[i] == ')')
                            {
                                bracketCloseCounter++;
                            }
                        }
                        BracketsCount += bracketCloseCounter - 1;

                        Expression = Expression.Remove(bracketOpenIndex);
                    }
                    if (_lastOperation == "=")
                    {
                        Result = "0";
                        Expression = string.Empty;
                    }
                    if (!Result.Contains("."))
                    {
                        Result += ".";
                    }

                    _lastOperation = "Number";
                });
            }
        }
        #endregion
        #region Operations ICommands
        //  Арифметические операции
        public ICommand ArithmeticOperationButtonPressCommand
        {
            get
            {
                return new RelayCommand<string>((operation) =>
                {
                    if (Result[^1] == ',')
                    {
                        Result = Result.Remove(Result.Length - 1);
                    }
                    if (_lastOperation == "=")
                    {
                        Expression = string.Empty;
                    }
                    if (_lastOperation == "Number" || _lastOperation == "=")
                    {
                        Expression += $"{Result} {operation} ";
                        Result = "0";
                    }
                    else if (_lastOperation == "BracketClose")
                    {
                        Expression += $"{operation} ";
                    }
                    else if (_lastOperation == "BracketOpen")
                    {
                        Expression += $"{Result} {operation} ";
                        Result = "0";
                    }
                    else
                    {
                        if (Expression == string.Empty)
                        {
                            Expression += $"{Result} ";
                        }
                        else
                        {
                            if ("+-–*×/÷".Contains(Expression[^2]))
                            {
                                Expression = Expression.Remove(Expression.Length - 2);
                            }
                        }

                        Expression += $"{operation} ";
                    }

                    _lastOperation = "Operation";
                });
            }
        }
        //  Вычисление
        public ICommand EqualButtonPressCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (Result[^1] == ',')
                    {
                        Result = Result.Remove(Result.Length - 1);
                    }
                    if (_lastOperation == "=")
                    {
                        Expression = $"{Result} =";
                    }
                    else
                    {
                        if (_lastOperation != "BracketClose")
                        {
                            Expression += $"{Result} ";
                        }
                        while (BracketsCount > 0)
                        {
                            Expression += ") ";
                            BracketsCount--;
                        }

                        Result = CalcModel.CalculatePriority(Expression);
                        Expression += "=";

                        Journal.Add(new JournalCalcItem(Expression, Result));
                    }

                    _lastOperation = "=";
                });
            }
        }
        //  Процент
        public ICommand PercentButtonPressCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (Result[^1] == ',')
                    {
                        Result = Result.Remove(Result.Length - 1);
                    }

                    string expression = Expression;
                    if (expression.Length > 2)
                    {
                        if ("+-–*×/÷".Contains(expression[^2]))
                        {
                            expression = expression.Remove(Expression.Length - 2);
                        }
                    }
                    for (int i = 0; i < BracketsCount; i++)
                    {
                        expression += ") ";
                    }

                    Result = CalcModel.Percent(expression, Result);
                });
            }
        }
        //  Инверсия значения
        public ICommand InvertButtonPressCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (Result[^1] == ',')
                    {
                        Result = Result.Remove(Result.Length - 1);
                    }

                    Result = CalcModel.Invert(Result);
                });
            }
        }
        //  Скобки приоритета
        public ICommand BraketsButtonPressCommand
        {
            get
            {
                return new RelayCommand<string>((bracket) =>
                {
                    switch (bracket)
                    {
                        case "(":
                            if (_lastOperation == "=")
                            {
                                Expression = string.Empty;
                            }
                            if (BracketsCount < 25 && _lastOperation != "BracketClose")
                            {
                                Expression += "( ";

                                BracketsCount++;
                                _lastOperation = "BracketOpen";
                            }
                            break;
                        case ")":
                            if (BracketsCount > 0)
                            {
                                if (Result[^1] == ',')
                                {
                                    Result = Result.Remove(Result.Length - 1);
                                }
                                if (_lastOperation != "BracketClose")
                                {
                                    Expression += $"{Result} ";
                                }

                                Expression += ") ";
                                Result = "0";

                                BracketsCount--;
                                _lastOperation = "BracketClose";
                            }
                            break;
                    }
                });
            }
        }
        #endregion
        #region App actions ICommands
        //  Очистка экранов (Result)
        public ICommand ClearButtonPressCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (_lastOperation == "=" || Result == "0")
                    {
                        Result = "0";
                        Expression = string.Empty;
                        BracketsCount = 0;
                    }
                    else
                    {
                        Result = "0";
                    }

                    _lastOperation = "Clear";
                });
            }
        }
        #endregion
        #region Journal ICommands
        //  Очистка журнала
        public ICommand JournalClearButtonPressCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Journal.Clear();
                });
            }
        }
        public ICommand JournalRecallButtonPressCommand
        {
            get
            {
                return new RelayCommand<JournalCalcItem>((journalItem) =>
                {
                    Result = journalItem.Result;
                    Expression = journalItem.Expression;
                    _lastOperation = "=";
                });
            }
        }
        #endregion
        #region Memory ICommands
        //  Извлечь последнее добавленное значение из памяти
        public ICommand MemoryRecallLastButtonPressCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (_lastOperation == "=")
                    {
                        Expression = string.Empty;
                    }
                    if (Memory.Collection.Count != 0)
                    {
                        Result = Memory.Collection[0].Number;
                        _lastOperation = "Number";
                    }
                }, () => Memory.Collection.Count != 0);
            }
        }
        //  Добавить к значению на дисплее последний добавленный элемент в память (Result += Memory[0])
        public ICommand MemoryAddLastButtonPressCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (_lastOperation == "=")
                    {
                        Expression = string.Empty;
                    }
                    if (Memory.Collection.Count != 0)
                    {
                        Result = CalcModel.CalculatePriority($"{Result} + {Memory.Collection[0].Number}");
                    }
                }, () => Memory.Collection.Count != 0);
            }
        }
        //  Вычесть из значения на дисплее последний добавленный элемент в память (Result -= Memory[0])
        public ICommand MemorySubtractLastButtonPressCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (_lastOperation == "=")
                    {
                        Expression = string.Empty;
                    }
                    if (Memory.Collection.Count != 0)
                    {
                        Result = CalcModel.CalculatePriority($"{Result} - {Memory.Collection[0].Number}");
                    }
                }, () => Memory.Collection.Count != 0);
            }
        }
        //  Сохранить значение в память
        public ICommand MemorySaveButtonPressCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Memory.Add(new MemoryCalcItem(Result));
                });
            }
        }
        //  Полная очистка памяти
        public ICommand MemoryClearAllButtonPressCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Memory.Clear();
                });
            }
        }
        //  Извлечь значение из памяти
        public ICommand MemoryRecallButtonPressCommand
        {
            get
            {
                return new RelayCommand<MemoryCalcItem>((memoryItem) =>
                {
                    if (_lastOperation == "=")
                    {
                        Expression = string.Empty;
                    }
                    Result = memoryItem.Number;

                    _lastOperation = "Number";
                });
            }
        }
        //  Удалить элемент из памяти
        public ICommand MemoryClearButtonPressCommand
        {
            get
            {
                return new RelayCommand<MemoryCalcItem>((memoryItem) =>
                {
                    Memory.Remove(memoryItem);
                });
            }
        }
        //  Добавить число на дисплее к ячейке памяти (Memory[index] += Result)
        public ICommand MemoryAddButtonPressCommand
        {
            get
            {
                return new RelayCommand<MemoryCalcItem>((memoryItem) =>
                {
                    Memory.ChangeValue(memoryItem, "Number", CalcModel.CalculatePriority($"{memoryItem.Number} + {Result}"));
                });
            }
        }
        //  Вычесть число на дисплее из ячейки памяти (Memory[index] -= Result)
        public ICommand MemorySubtractButtonPressCommand
        {
            get
            {
                return new RelayCommand<MemoryCalcItem>((memoryItem) =>
                {
                    Memory.ChangeValue(memoryItem, "Number", CalcModel.CalculatePriority($"{memoryItem.Number} - {Result}"));
                });
            }
        }
        #endregion
    }
}