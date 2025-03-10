using Chat_App;
using Client__.Net_.MVVM.View;
using Client__.Net_.MVVM.ViewModel;
using System;
using System.Windows;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Windows.Media;

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

        public void ChangePrimaryColor(string colorName)
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
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

           

            var dbService = new SQLiteDBService();

            var viewModel = new LoginViewModel();
       
            SetPrimaryColorFromUserSelection(dbService);
            
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

        public void SetPrimaryColorFromUserSelection(SQLiteDBService sqliteService)
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
