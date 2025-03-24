using Chat_App;
using Client__.Net_.MVVM.View;
using Client__.Net_.MVVM.ViewModel;
using System;
using System.Windows;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Windows.Media;
using System.Windows.Threading;

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

        // Mapping from stored hex value to MaterialDesign swatch name.
        private static readonly Dictionary<string, string> HexToSwatch = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "#FF9E0808", "Red" },
            { "#FF008000", "Green" },
            { "#FF1E90FF", "Blue" },
            { "#FFE5AF09", "Yellow" },
            { "#FF561974", "Purple" }
        };


        public App()
        {
            // Prevent the app from shutting down when the main window closes.
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }


        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Show the splash screen
            var splashScreen = new MVVM.View.SplashScreen();
            splashScreen.Show();

            // Initialize the MainViewModel
            var mainViewModel = new MainViewModel();
            _ = Task.Run(mainViewModel.LoadUserGroupsAsync); // Run in background

            // Run initialization tasks in the background and capture the required startup state.
            var initResult = await Task.Run(() =>
            {
                var dbService = new SQLiteDBService();
                var viewModel = new LoginViewModel();

                // Subscribe to events for further navigation if needed.
                viewModel.OnSettingsCompleted += OnSettingsCompleted;
                viewModel.OnUserLoginCompleted += OnUserLoginCompleted;

                // Check if the Settings table has data.
                bool settingsDataPresent = dbService.TableHasData("settings");
                bool openSettings = !settingsDataPresent;

                bool isUserDataPresent = false;
                bool isUserLoggedIn = false;
                if (settingsDataPresent)
                {
                    var (userDataPresent, _) = dbService.CheckInitializationState();
                    isUserDataPresent = userDataPresent;
                    if (userDataPresent)
                    {
                        isUserLoggedIn = dbService.IsUserLoggedIn();
                    }
                }

                return (viewModel, openSettings, isUserDataPresent, isUserLoggedIn);
            });

            // Ensure the splash screen is visible for at least 5 seconds.
            await Task.Delay(5000);
            splashScreen.Close();

            // Sequentially open windows based on initialization.
            if (initResult.openSettings)
            {
                _settingsWindow = new Settings { DataContext = initResult.viewModel };
                _settingsWindow.ShowDialog();

                _userLoginWindow = new UserLogin { DataContext = initResult.viewModel };
                _userLoginWindow.ShowDialog();
            }
            else if (initResult.isUserDataPresent && initResult.isUserLoggedIn)
            {
                OpenMainWindow();
            }
            else
            {
                _userLoginWindow = new UserLogin { DataContext = initResult.viewModel };
                _userLoginWindow.ShowDialog();
            }
        }


        public static void SetPrimaryColorFromUserSelection(SQLiteDBService sqliteService)
        {
            var user = sqliteService.LoadUser();

            // Default to Blue if no valid selection is found.
            string swatchName = "Green";

            if (user != null && !string.IsNullOrWhiteSpace(user.SelectedColor))
            {
                // Check if the stored hex value matches one of our allowed values.
                if (HexToSwatch.TryGetValue(user.SelectedColor, out var mappedSwatch))
                {
                    swatchName = mappedSwatch;
                }
            }

            // Apply the primary color using the swatch name.
            ChangePrimaryColor(swatchName);
        }

        public static void ChangePrimaryColor(string colorName)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            // Use SwatchesProvider to get all available swatches
            var provider = new SwatchesProvider();
            // Look for the swatch whose Name matches the provided colorName (case-insensitive)
            var swatch = provider.Swatches.FirstOrDefault(s =>
                s.Name.Equals(colorName, StringComparison.InvariantCultureIgnoreCase));

            if (swatch != null)
            {
                // Use the exemplar hue from the swatch.
                // This hue represents a mid-range color that is typically used as the primary color.
                var primaryHue = swatch.ExemplarHue;
                if (primaryHue != null)
                {
                    Color primaryColor = primaryHue.Color;
                    // Set the primary color in the theme and update it
                    theme.SetPrimaryColor(primaryColor);
                    paletteHelper.SetTheme(theme);
                }
                else
                {
                    MessageBox.Show(
                        $"The swatch '{colorName}' does not have an exemplar hue.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(
                    $"Invalid color name: {colorName}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            var dbService = new SQLiteDBService();

            // Apply user settings (primary color)
            SetPrimaryColorFromUserSelection(dbService);

            _mainWindow = new MainWindow();
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Focus();
        }
    }
}
