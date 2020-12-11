using FluentAssertions;
using NUnit.Framework;

namespace Calculator.Tests
{
    [TestFixture]
    public class CalculatorViewModelTests
    {
        private CalcViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            _viewModel = new CalcViewModel();
        }

        [Test]
        public void Test_minus3add17_14display()
        {
            _viewModel.DigitButtonPress("3");
            _viewModel.InvertButtonPress();
            _viewModel.Result.Should().Be("-3");

            _viewModel.ArithmeticOperationButtonPress("+");
            _viewModel.LastOperation.Should().Be("+");

            _viewModel.DigitButtonPress("1");
            _viewModel.DigitButtonPress("7");
            _viewModel.Result.Should().Be("17");

            _viewModel.EqualButtonPress();
            _viewModel.Expression.Should().Be("-3 + 17 =");
            _viewModel.Result.Should().Be("14");
        }
        [Test]
        public void Test_clear()
        {
            Test_minus3add17_14display();

            _viewModel.ClearButtonPress();
            _viewModel.Result.Should().Be("0");
            _viewModel.Expression.Should().Be(string.Empty);
        }
    }
}