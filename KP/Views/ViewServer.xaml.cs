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
        private void ListBox_files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((ViewmodelServer)ViewModelMain.View_Model_Main.viewmodels[1]).ListBox_files_SelectionChanged(sender, e);
        }
        private void ListBox_users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((ViewmodelServer)ViewModelMain.View_Model_Main.viewmodels[1]).ListBox_users_SelectionChanged(sender, e);
        }
    }
}