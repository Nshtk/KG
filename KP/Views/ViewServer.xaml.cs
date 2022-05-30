using System.Windows.Controls;
using KP.ViewModels;

namespace KP.Views
{
    public partial class ViewServer : UserControl
    {
        public ViewServer()
        {
            InitializeComponent();
            DataContext=ViewModelMain.View_Model_Main.viewmodels[1];
        }
    }
}