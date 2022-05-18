using System;
using System.IO;
using System.Windows;
using KP.ViewModels;
using Microsoft.Win32;

namespace KP.Views
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext=new ViewModelMain();
        }
    }
}