using FluentAssertions;
using NUnit.Framework;

namespace Calculator.Tests
{
    [TestFixture]
    public class CalculatorModelTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test, Category("Addition")]
        public void Add_10and20_30return()
        {
            CalcModel.CalculatePriority("10 + 20").Should().Be("30");
        }
        [Test, Category("Addition")]
        public void Add_minus15and130_115return()
        {
            CalcModel.CalculatePriority("-15 + 130").Should().Be("115");
        }
        [Test, Category("Subtraction")]
        public void Sub_26and16_10return()
        {
            CalcModel.CalculatePriority("26 - 16").Should().Be("10");
        }
        [Test, Category("Subtraction")]
        public void Sub_minus1574and6_minus1580return()
        {
            CalcModel.CalculatePriority("-1574 - 6").Should().Be("-1580");
        }
        [Test, Category("Multiplication")]
        public void Mult_7and5_35return()
        {
            CalcModel.CalculatePriority("7 * 5").Should().Be("35");
        }
        [Test, Category("Multiplication")]
        public void Mult_minus548and0_0return()
        {
            CalcModel.CalculatePriority("548 * 0").Should().Be("0");
        }
        [Test, Category("Division")]
        public void Div_15and3_5return()
        {
            CalcModel.CalculatePriority("15 / 3").Should().Be("5");
        }
        [Test, Category("Inversion")]
        public void Invert_minus5_5return()
        {
            CalcModel.Invert("-5").Should().Be("5");
        }
        [Test, Category("Percent")]
        public void Percent_10of250_25return()
        {
            CalcModel.Percent("250", "10").Should().Be("25");
        }
        [Test, Category("Percent")]
        public void Percent_100_1return()
        {
            CalcModel.Percent("100", "1").Should().Be("1");
        }
        [Test, Category("Common")]
        public void AllOperations_BigExpression_53return()
        {
            CalcModel.CalculatePriority("( 1 + 4 ) * 5 - 7 + ( 5 - 3 + ( 9 - 10 ) * 2 ) / 9 + ( ( 9 + 5 * 2 ) - 25 / 5 ) + 85 - 32 * 2").Should().Be("53");
        }
    }
}