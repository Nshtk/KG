using System.Windows.Controls;
using KP.ViewModels;

namespace KP.Views
{
    public partial class ViewClient : UserControl
    {
        public ViewClient()
        {
            InitializeComponent();
            DataContext=ViewModelMain.View_Model_Main.viewmodels[0];
        }
    }
}