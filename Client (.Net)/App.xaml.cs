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
        private UserProfile _userProfileWindow;
        private Settings _settingsWindow;
        private MainWindow _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var dbService = new SQLiteDBService();
            dbService.InitializeDatabase();

            // Check if user data and settings are present
            var (isUserDataPresent, isSettingsDataPresent) = dbService.CheckInitializationState();

            var viewModel = new MainViewModel();
            viewModel.ProfileCompleted += OnProfileCompleted;
            viewModel.SettingsCompleted += OnSettingsCompleted;

            // Check if user data is present, if not show the UserProfile window
            if (!isUserDataPresent)
            {
                _userProfileWindow = new UserProfile
                {
                    DataContext = viewModel
                };
                _userProfileWindow.ShowDialog();  // Show the UserProfile window

                // After the UserProfile window is completed, check again
                (isUserDataPresent, isSettingsDataPresent) = dbService.CheckInitializationState();
            }

            // Check if settings data is present, if not show the Settings window
            if (!isSettingsDataPresent)
            {
                _settingsWindow = new Settings
                {
                    DataContext = viewModel
                };
                _settingsWindow.ShowDialog();  // Show the Settings window

                // After the Settings window is completed, check again
                (isUserDataPresent, isSettingsDataPresent) = dbService.CheckInitializationState();
            }

            // After user data and settings are completed, show the main window
            if (isUserDataPresent && isSettingsDataPresent)
            {
                // Ensure the MainWindow is only created once
                if (_mainWindow == null)
                {
                    _mainWindow = new MainWindow();
                    _mainWindow.Show();  // Show the main window

                    // Explicitly set the MainWindow state to Normal and focus it
                    _mainWindow.WindowState = WindowState.Normal;
                    _mainWindow.Focus();  // Set focus to the main window to ensure it's active
                }
            }
        }

        private void OnProfileCompleted(object sender, EventArgs e)
        {
            // Handle ProfileCompleted: Show the Settings window
            var dbService = new SQLiteDBService();
            var (isUserDataPresent, isSettingsDataPresent) = dbService.CheckInitializationState();

            // Hide the UserProfile window before opening Settings window
            if (_userProfileWindow != null && _userProfileWindow.IsVisible)
            {
                _userProfileWindow.Hide();  // Hide the UserProfile window
            }

            // Open Settings window if settings are not completed
            if (!isSettingsDataPresent)
            {
                _settingsWindow = new Settings
                {
                    DataContext = (MainViewModel)sender
                };
                _settingsWindow.ShowDialog();
            }
        }

        private void OnSettingsCompleted(object sender, EventArgs e)
        {
            // Handle SettingsCompleted: Hide the Settings window and show MainWindow
            if (_settingsWindow != null && _settingsWindow.IsVisible)
            {
                _settingsWindow.Hide();  // Hide the Settings window
            }

            // Open the MainWindow after Settings is completed
            // Ensure that the MainWindow is only created once
            if (_mainWindow == null)
            {
                _mainWindow = new MainWindow();
                _mainWindow.Show();

                // Explicitly set the MainWindow state to Normal and focus it
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Focus();  // Set focus to the main window to ensure it's active
            }
        }
    }
}
