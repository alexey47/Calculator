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
        public void Test_minus3add17_result14()
        {
            _viewModel.DigitButtonPressCommand.Execute("3");
            _viewModel.InvertButtonPressCommand.Execute(null);
            _viewModel.Result.Should().Be("-3");

            _viewModel.ArithmeticOperationButtonPressCommand.Execute("+");

            _viewModel.DigitButtonPressCommand.Execute("1");
            _viewModel.DigitButtonPressCommand.Execute("7");
            _viewModel.Result.Should().Be("17");

            _viewModel.EqualButtonPressCommand.Execute(null);
            _viewModel.Expression.Should().Be("-3 + 17 =");
            _viewModel.Result.Should().Be("14");
        }
        [Test]
        public void Test_clear()
        {
            Test_minus3add17_result14();

            _viewModel.ClearButtonPressCommand.Execute(null);
            _viewModel.Result.Should().Be("0");
            _viewModel.Expression.Should().Be(string.Empty);
        }
    }
}