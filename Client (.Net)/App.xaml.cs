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

            // Step 1: Check if Settings table has data
            bool settingsDataPresent = dbService.TableHasData("settings");

            if (!settingsDataPresent)
            {
                // Open Settings window if no settings data is present
                _settingsWindow = new Settings
                {
                    DataContext = viewModel
                };
                _settingsWindow.ShowDialog();
            }

            // Step 2: Check if user data exists in users table
            var (isUserDataPresent, _) = dbService.CheckInitializationState();

            if (isUserDataPresent)
            {
                // Step 3: Check if UserLogin is true (1) in login table
                bool isUserLoggedIn = dbService.IsUserLoggedIn();

                if (isUserLoggedIn)
                {
                    // Open MainWindow if user is already logged in
                    OpenMainWindow();
                    return;
                }
            }

            // If user is not logged in or no user exists, open UserLogin window
            _userLoginWindow = new UserLogin
            {
                DataContext = viewModel
            };
            _userLoginWindow.ShowDialog();
        }

        private void OnSettingsCompleted(object sender, EventArgs e)
        {
            if (_settingsWindow != null && _settingsWindow.IsVisible)
            {
                _settingsWindow.Hide();
            }
        }

        private void OnUserLoginCompleted(object sender, EventArgs e)
        {
            if (_userLoginWindow != null && _userLoginWindow.IsVisible)
            {
                _userLoginWindow.Hide();
            }

            // Open MainWindow after login is completed
            OpenMainWindow();
        }

        private void OpenMainWindow()
        {
            _mainWindow = new MainWindow();
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Focus();
        }
    }
}
