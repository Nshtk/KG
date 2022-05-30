using KP.ViewModels;

namespace KP.Views
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext=new ViewModelMain(); //System.Diagnostics.Trace.WriteLine($"{}");
        }
    }
}