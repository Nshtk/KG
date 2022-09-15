using System;
using Soldatov.Wpf.MVVM.Core;

namespace KP.ViewModels
{
    public class ViewModelMain : ViewModelBase
    {
        #region Types
        public enum ViemodelName
        {
            CLIENT,
            SERVER
        }
        #endregion

        #region Fields
        public static ViewModelMain View_Model_Main;    // TODO check if property 100% doesnt work
        public object[] viewmodels=new object[] {new ViewmodelClient(), new ViewmodelServer()};
        private object _current_viewmodel;
        #endregion

        #region Fields_WPF
        private RelayCommand _command_switch_view;
        #endregion

        #region Properties
        public object CurrentViewmodel
        {
            get {return _current_viewmodel;}
            set {_current_viewmodel=value; invokePropertyChanged("CurrentViewmodel");}
        }
        #endregion
        
        #region Properties_WPF
        public RelayCommand CommandSwitchView
        {
            get {return _command_switch_view??=new RelayCommand(switchView_execute, switchView_canExecute);}
        }
        #endregion
        
        public ViewModelMain()
        {
            CurrentViewmodel=viewmodels[0];
            View_Model_Main=this;
        }

        #region Command_methods
        private void switchView_execute(object parameter)
        {
            switch((ViemodelName)parameter)
            {
                case ViemodelName.CLIENT:
                    CurrentViewmodel=viewmodels[0];
                    break;
                case ViemodelName.SERVER:
                    CurrentViewmodel=viewmodels[1];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
            }
        }
        private bool switchView_canExecute(object parameter)
        {
            return true;
        }
        #endregion
    }
}