using Chat_App;
using Chat_App.MVVM.ViewModel;
using Client__.Net_.MVVM.View;
using System;
using System.Windows;

namespace Client__.Net_
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private UserLogin _userLoginWindow;
        private Settings _settingsWindow;
        private MainWindow _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var dbService = new SQLiteDBService();
            dbService.InitializeDatabase();

            var viewModel = new MainViewModel();
            viewModel.OnSettingsCompleted += OnSettingsCompleted;
            viewModel.OnUserLoginCompleted += OnUserLoginCompleted;

            // Check if Settings table has data
            var settingsDataPresent = dbService.TableHasData("settings");

            if (!settingsDataPresent)
            {
                // Open Settings window if no settings data is present
                _settingsWindow = new Settings
                {
                    DataContext = viewModel
                };
                _settingsWindow.ShowDialog();  // Show the Settings window first
            }

            // Check if user data exists in users table
            var (isUserDataPresent, _) = dbService.CheckInitializationState();

            if (!isUserDataPresent)
            {
                // If no user data, open the UserLogin window
                _userLoginWindow = new UserLogin
                {
                    DataContext = viewModel
                };
                _userLoginWindow.ShowDialog();  // Show the UserLogin window
            }
            else
            {
                // Check if UserLogin is false (0) in login table
                bool isUserLoggedIn = dbService.IsUserLoggedIn();

                if (!isUserLoggedIn)
                {
                    // Open UserLogin window if UserLogin is false
                    _userLoginWindow = new UserLogin
                    {
                        DataContext = viewModel
                    };
                    _userLoginWindow.ShowDialog();  // Show the UserLogin window
                }
            }

            //// Once both settings and user login are done, open MainWindow
            //if (_mainWindow == null)
            //{
            //    _mainWindow = new MainWindow();
            //    _mainWindow.Show();
            //    _mainWindow.WindowState = WindowState.Normal;
            //    _mainWindow.Focus();
            //}
        }


        private void OnSettingsCompleted(object sender, EventArgs e)
        {
            // Hide the Settings window when completed
            if (_settingsWindow != null && _settingsWindow.IsVisible)
            {
                _settingsWindow.Hide();
            }
        }

        private void OnUserLoginCompleted(object sender, EventArgs e)
        {
            // Hide the UserLogin window when completed
            if (_userLoginWindow != null && _userLoginWindow.IsVisible)
            {
                _userLoginWindow.Hide();
            }
        }
    }
}
