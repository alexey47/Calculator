using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;

namespace Calculator
{
    public partial class CalcView
    {
        public CalcView()
        {
            InitializeComponent();

            DataContext = new CalcViewModel();
        }

        private void ShowCollapseJournalList(object sender, RoutedEventArgs e)
        {
            if (JournalArea.Visibility == Visibility.Collapsed)
            {
                MemoryArea.Visibility = Visibility.Collapsed;
                JournalArea.Visibility = Visibility.Visible;
                KeyboardArea.Effect = new BlurEffect { Radius = 30 };
            }
            else
            {
                JournalArea.Visibility = Visibility.Collapsed;
                KeyboardArea.Effect = new BlurEffect { Radius = 0 };
            }
        }
        private void ShowCollapseMemoryList(object sender, RoutedEventArgs e)
        {
            if (MemoryArea.Visibility == Visibility.Collapsed)
            {
                MemoryArea.Visibility = Visibility.Visible;
                JournalArea.Visibility = Visibility.Collapsed;
                KeyboardArea.Effect = new BlurEffect { Radius = 30 };
            }
            else
            {
                MemoryArea.Visibility = Visibility.Collapsed;
                KeyboardArea.Effect = new BlurEffect { Radius = 0 };
            }
        }
        private void ShowCollapseMemoryQuickPanel(object sender, RoutedEventArgs e)
        {
            if (MemoryQuickPanel.Visibility == Visibility.Collapsed)
            {
                MemoryQuickPanel.Visibility = Visibility.Visible;
            }
            else
            {
                MemoryQuickPanel.Visibility = Visibility.Collapsed;
                if (MemoryArea.Visibility == Visibility.Visible)
                {
                    MemoryArea.Visibility = Visibility.Collapsed;
                    KeyboardArea.Effect = new BlurEffect { Radius = 0 };
                }
            }
        }
        private void ShowCollapseBracketsMenu(object sender, RoutedEventArgs e)
        {
            BracketsMenu.HorizontalOffset = -DigitZero.ActualWidth - 15;
            BracketsMenu.VerticalOffset = -(DigitZero.ActualHeight / 2);

            BracketsMenu.IsOpen = !BracketsMenu.IsOpen;
        }
        private void BracketExecuteCommand(object sender, MouseButtonEventArgs e)
        {
            ((Button)sender).Command.Execute(((Button)sender).CommandParameter);

            BracketsMenu.IsOpen = !BracketsMenu.IsOpen;
        }
    }
}

