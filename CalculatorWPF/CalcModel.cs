using System;
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
            expression = expression.Replace('×', '*').Replace('÷', '/');
            string[] exp = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string[] operations = { "+", "-", "*", "/" };

            for (int i = 1; i < exp.Length - 1; i += 2)
            {
                if (operations.Contains(exp[i]))
                {
                    if (!double.TryParse(exp[i - 1], out _))
                    {
                        exp[i - 1] = NormalizeNumber(exp[i - 1]);
                    }
                    if (!double.TryParse(exp[i + 1], out _))
                    {
                        exp[i + 1] = NormalizeNumber(exp[i + 1]);
                    }

                    switch (exp[i])
                    {
                        case "+":
                            exp[i + 1] = (double.Parse(exp[i - 1]) + double.Parse(exp[i + 1])).ToString(CultureInfo.CurrentCulture);
                            break;
                        case "-":
                            exp[i + 1] = (double.Parse(exp[i - 1]) - double.Parse(exp[i + 1])).ToString(CultureInfo.CurrentCulture);
                            break;
                        case "*":
                            exp[i + 1] = (double.Parse(exp[i - 1]) * double.Parse(exp[i + 1])).ToString(CultureInfo.CurrentCulture);
                            break;
                        case "/":
                            exp[i + 1] = (double.Parse(exp[i - 1]) / double.Parse(exp[i + 1])).ToString(CultureInfo.CurrentCulture);
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
            if (!char.IsDigit(number[^2]))
            {
                number = number.Remove(number.Length - 2);
            }
            if (number.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 2)
            {
                number = Calculate(number);
            }
            if (!double.TryParse(number, out _))
            {
                number = NormalizeNumber(number);
            }
            if (!double.TryParse(part, out _))
            {
                part = NormalizeNumber(part);
            }

            return (double.Parse(number) * double.Parse(part) * 0.01).ToString(CultureInfo.CurrentCulture);
        }
        /// <summary>
        /// Number inversion.
        /// </summary>
        /// <param name="number"></param>
        /// <returns>Inverted number (string)</returns>
        public static string Invert(string number)
        {
            if (double.TryParse(number, out _))
            {
                return (-double.Parse(number)).ToString(CultureInfo.CurrentCulture);
            }
            number = NormalizeNumber(number);
            return (-double.Parse(number)).ToString(CultureInfo.CurrentCulture);
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
            
            return double.TryParse(number, out _) ? number : "0";
        }
    }
}