using FluentAssertions;
using NUnit.Framework;

namespace Calculator.Tests
{
    public class CalculatorModelTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Add_10and20_30return()
        {
            CalcModel.Calculate("10 + 20").Should().Be("30");
        }
        [Test]
        public void Sub_26and16_10return()
        {
            CalcModel.Calculate("26 - 16").Should().Be("10");
        }
        [Test]
        public void Mult_7and5_35return()
        {
            CalcModel.Calculate("7 × 5").Should().Be("35");
        }
        [Test]
        public void Div_15and3_5return()
        {
            CalcModel.Calculate("15 ÷ 3").Should().Be("5");
        }
        [Test]
        public void Invert_minus5_5return()
        {
            CalcModel.Invert("-5").Should().Be("5");
        }
        [Test]
        public void Percent_10of250_25return()
        {
            CalcModel.Percent("250", "10").Should().Be("25");
        }
        [Test]
        public void Percent_100_1return()
        {
            CalcModel.Percent("100", "1").Should().Be("1");
        }
    }
}