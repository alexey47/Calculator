using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Calculator
{
    public class CalcModel
    {
        /// <summary>
        /// Expression format: <i>N O N [O N ... O N]</i> (N - number, O - operation).<br/>
        /// Available operations: <i>+, -, *, / </i>.<br/>
        /// </summary>
        /// <param name="expression"></param>
        /// <returns>Expression result (string)</returns>
        public static string Calculate(string expression)
        {
            expression = expression.Replace('×', '*').Replace('÷', '/').Replace('–', '-');
            string[] exp = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string[] operations = { "+", "-", "*", "/" };

            for (int i = 1; i < exp.Length - 1; i += 2)
            {
                if (operations.Contains(exp[i]))
                {
                    if (!decimal.TryParse(exp[i - 1], out _))
                    {
                        exp[i - 1] = NormalizeNumber(exp[i - 1]);
                    }
                    if (!decimal.TryParse(exp[i + 1], out _))
                    {
                        exp[i + 1] = NormalizeNumber(exp[i + 1]);
                    }

                    switch (exp[i])
                    {
                        case "+":
                            exp[i + 1] = (decimal.Parse(exp[i - 1]) + decimal.Parse(exp[i + 1])).ToString(CultureInfo.CurrentCulture);
                            break;
                        case "-":
                            exp[i + 1] = (decimal.Parse(exp[i - 1]) - decimal.Parse(exp[i + 1])).ToString(CultureInfo.CurrentCulture);
                            break;
                        case "*":
                            exp[i + 1] = (decimal.Parse(exp[i - 1]) * decimal.Parse(exp[i + 1])).ToString(CultureInfo.CurrentCulture);
                            break;
                        case "/":
                            exp[i + 1] = (decimal.Parse(exp[i - 1]) / decimal.Parse(exp[i + 1])).ToString(CultureInfo.CurrentCulture);
                            break;
                    }
                }
                else
                {
                    exp[^1] = "Error!";
                    break;
                }
            }
            return exp[^1];
        }
        /// <summary>
        /// Variant with calculation priority.<br/>
        /// Expression format: <i>N O N [O N ... O N]</i> (N - number, O - operation).<br/>
        /// Available operations: <i>+, -, *, /, ), (</i>.<br/>
        /// </summary>
        /// <param name="expression"></param>
        /// <returns>Expression result (string)</returns>
        public static string CalculatePriority(string expression)
        {
            ReadOnlyDictionary<string, int> operatorsPriority = new ReadOnlyDictionary<string, int>(
                new Dictionary<string, int>
                {
                    {"*", 0},
                    {"/", 0},
                    {"+", 1},
                    {"-", 1},
                    {"(", 2},
                    {")", 2}
                });

            //  Подготовка: Замена кастом символов операций на обычные
            expression = expression.Replace('×', '*').Replace('÷', '/').Replace('–', '-');

            //  Подготовка: Нормализация чисел
            string[] exp = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < exp.Length; i++)
            {
                if (!operatorsPriority.ContainsKey(exp[i]) && !decimal.TryParse(exp[i], out _))
                {
                    exp[i] = NormalizeNumber(exp[i]);
                }
            }

            //  Подготовка: Перевод выражения в постфиксную нотацию
            #region Convert To Postfix Notation

            expression = string.Empty;
            Stack<string> operations = new Stack<string>();
            foreach (var item in exp)
            {
                if (decimal.TryParse(item, out _))
                {
                    expression += $"{item} ";
                }
                else if (item == "(")
                {
                    operations.Push(item);
                }
                else if (item == ")")
                {
                    while (operations.Peek() != "(")
                    {
                        expression += $"{operations.Pop()} ";
                    }
                    if (operations.Peek() == "(")
                    {
                        operations.Pop();
                    }
                }
                else if (operatorsPriority.ContainsKey(item))
                {
                    if (!operations.Count.Equals(0) && operatorsPriority[item] >= operatorsPriority[operations.Peek()])
                    {
                        expression += $"{operations.Pop()} ";
                    }
                    operations.Push(item);
                }
            }
            while (!operations.Count.Equals(0))
            {
                expression += $"{operations.Pop()} ";
            }
            
            #endregion

            //  Подготовка: Перемещение в List для более удобного удаления элементов
            List<string> expList = new List<string>(expression.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            //  Расчет
            for (int i = 2; i < expList.Count; i++)
            {
                if (operatorsPriority.ContainsKey(expList[i]))
                {
                    switch (expList[i])
                    {
                        case "+":
                            expList[i] = (decimal.Parse(expList[i - 2]) + decimal.Parse(expList[i - 1])).ToString(CultureInfo.CurrentCulture);
                            break;
                        case "-":
                            expList[i] = (decimal.Parse(expList[i - 2]) - decimal.Parse(expList[i - 1])).ToString(CultureInfo.CurrentCulture);
                            break;
                        case "*":
                            expList[i] = (decimal.Parse(expList[i - 2]) * decimal.Parse(expList[i - 1])).ToString(CultureInfo.CurrentCulture);
                            break;
                        case "/":
                            expList[i] = (decimal.Parse(expList[i - 2]) / decimal.Parse(expList[i - 1])).ToString(CultureInfo.CurrentCulture);
                            break;
                    }
                    expList.RemoveRange(i - 2, 2);
                    i -= 2;
                }
            }

            return expList[0];
        }
        /// <summary>
        /// Percent.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="part"></param>
        /// <returns>Part of number (string)</returns>
        public static string Percent(string number, string part)
        {
            if (number.Length == 0)
            {
                return "0";
            }
            if (number.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 2)
            {
                number = CalculatePriority(number);
            }
            if (!decimal.TryParse(number, out _))
            {
                number = NormalizeNumber(number);
            }
            if (!decimal.TryParse(part, out _))
            {
                part = NormalizeNumber(part);
            }

            return (decimal.Parse(number) * decimal.Parse(part) / 100).ToString(CultureInfo.CurrentCulture);
        }
        /// <summary>
        /// Number inversion.
        /// </summary>
        /// <param name="number"></param>
        /// <returns>Inverted number (string)</returns>
        public static string Invert(string number)
        {
            if (!decimal.TryParse(number, out _))
            {
                number = NormalizeNumber(number);
            }

            return (-decimal.Parse(number)).ToString(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Try to transform "smoker's number" to "normal number".
        /// </summary>
        /// <param name="number"></param>
        /// <returns>Success: number<br/>
        ///          Fail: 0</returns>
        private static string NormalizeNumber(string number)
        {
            if (number == string.Empty)
            {
                return "0";
            }

            char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            int firstDigitIdx = number.IndexOfAny(digits);

            if (firstDigitIdx != -1)
            {
                #region IsNegative

                int minusIdx = number.IndexOf('-', 0, firstDigitIdx);
                bool isNegative = minusIdx != -1;

                #endregion
                #region IsExp

                int negativeExpIdx = number.IndexOf("E-", firstDigitIdx, StringComparison.Ordinal);
                int positiveExpIdx = number.IndexOf("E+", firstDigitIdx, StringComparison.Ordinal);

                bool isExp = false;

                Tuple<int, string> expIdx = new Tuple<int, string>(0, string.Empty);
                if (negativeExpIdx != -1 || positiveExpIdx != -1)
                {
                    if (negativeExpIdx != -1 && positiveExpIdx != -1)
                    {
                        expIdx = negativeExpIdx < positiveExpIdx
                            ? Tuple.Create(negativeExpIdx, "E-")
                            : Tuple.Create(positiveExpIdx, "E+");
                    }
                    else if (negativeExpIdx != -1)
                    {
                        expIdx = Tuple.Create(negativeExpIdx, "E-");
                    }
                    else if (positiveExpIdx != -1)
                    {
                        expIdx = Tuple.Create(positiveExpIdx, "E+");
                    }

                    isExp = true;
                }

                #endregion
                #region IsDouble

                int pointIdx = isExp
                    ? number.IndexOf(',', firstDigitIdx, expIdx.Item1 - firstDigitIdx - 1)
                    : number.IndexOf(',', firstDigitIdx);
                bool isDouble = pointIdx != -1;

                #endregion

                #region NewPositions

                int expNewIdx = 0;
                if (isExp)
                {
                    for (int i = 0; i < expIdx.Item1; i++)
                    {
                        if (char.IsDigit(number[i]))
                        {
                            expNewIdx++;
                        }
                    }
                    expIdx = Tuple.Create(expNewIdx, expIdx.Item2);
                }

                int pointNewIdx = 0;
                if (isDouble)
                {
                    for (int i = firstDigitIdx; i < pointIdx; i++)
                    {
                        if (char.IsDigit(number[i]))
                        {
                            pointNewIdx++;
                        }
                    }
                    pointIdx = pointNewIdx;
                }

                #endregion

                number = new string(number.Where(char.IsDigit).ToArray());
                if (isExp)
                {
                    number = number.Insert(expIdx.Item1, expIdx.Item2);
                }
                if (isDouble)
                {
                    number = number.Insert(pointIdx, ",");
                }
                if (isNegative)
                {
                    number = number.Insert(0, "-");
                }
            }

            return decimal.TryParse(number, out _) ? number : "0";
        }
    }
}