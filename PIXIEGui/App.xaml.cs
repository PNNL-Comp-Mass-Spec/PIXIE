using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PIXIEGui
{
    using PIXIEGui.ViewModels;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var mainViewModel = new MainWindowViewModel();
            var mainWindow = new Views.MainWindow { DataContext = mainViewModel };
            mainWindow.Show();
        }
    }
}
