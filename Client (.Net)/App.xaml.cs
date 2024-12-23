using Chat_App.MVVM.ViewModel;
using Chat_App;
using Client__.Net_.MVVM.ViewModel;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Client__.Net_
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var mainViewModel = new MainViewModel();
            var settingsViewModel = new SettingsViewModel();

            var mainWindow = new MainWindow { DataContext = mainViewModel };
            //mainWindow.Show();

            base.OnStartup(e);
        }

    }

}
