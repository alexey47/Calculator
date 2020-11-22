using System;
using System.Globalization;
using System.Linq;

namespace Calculator
{
    public class CalcModel
    {
        public static string Calculate(string expression)
        {
            expression = expression.Replace('×', '*').Replace('÷', '/');
            string[] exp = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string[] operations = { "+", "-", "*", "/" };

            for (int i = 1; i < exp.Length - 1; i += 2)
            {
                bool errorFlag = false;

                if (operations.Contains(exp[i]))
                {
                    switch (exp[i])
                    {
                        case "+":
                            exp[i + 1] =
                                (double.Parse(exp[i - 1]) + double.Parse(exp[i + 1])).ToString(CultureInfo
                                    .CurrentCulture);
                            break;
                        case "-":
                            exp[i + 1] =
                                (double.Parse(exp[i - 1]) - double.Parse(exp[i + 1])).ToString(CultureInfo
                                    .CurrentCulture);
                            break;
                        case "*":
                            exp[i + 1] =
                                (double.Parse(exp[i - 1]) * double.Parse(exp[i + 1])).ToString(CultureInfo
                                    .CurrentCulture);
                            break;
                        case "/":
                            exp[i + 1] =
                                (double.Parse(exp[i - 1]) / double.Parse(exp[i + 1])).ToString(CultureInfo
                                    .CurrentCulture);
                            break;
                        default:
                            exp[^1] = "Error!";
                            errorFlag = true;
                            break;
                    }
                }
                if (errorFlag)
                {
                    break;
                }
            }
            return exp[^1];
        }
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

            return (double.Parse(number) * double.Parse(part) * 0.01).ToString(CultureInfo.CurrentCulture);
        }
        public static string Invert(string number)
        {
            return (-double.Parse(number)).ToString(CultureInfo.CurrentCulture);
        }
    }
}