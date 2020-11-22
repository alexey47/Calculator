namespace Calculator
{
    public partial class CalcView
    {
        public CalcView()
        {
            InitializeComponent();

            DataContext = new CalcViewModel();
        }

    }
}

