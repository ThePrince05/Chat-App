using Chat_App;
using Client__.Net_.MVVM.View;
using Client__.Net_.MVVM.ViewModel;
using System;
using System.Windows;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Windows.Media;
using System.Windows.Threading;
using Client__.Net_.Services;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;

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

        public static MessageTrackerService MessageTrackerService { get; private set; }
        public static NotifyIcon NotifyIconInstance { get; private set; }

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

            MessageTrackerService = new MessageTrackerService();
            if (NotifyIconInstance == null)
            {
                NotifyIconInstance = new NotifyIcon
                {
                    Icon = new System.Drawing.Icon("Assets/Icons/group.ico"),
                    Visible = true,
                    Text = "Chat App"
                };
            }

            var splashScreen = new MVVM.View.SplashScreen();
            splashScreen.Show();

            var mainViewModel = new MainViewModel();
            var loadGroupsTask = mainViewModel.LoadUserGroupsAsync(); // Start fetching groups

            var initTask = Task.Run(() =>
            {
                var dbService = new SQLiteDBService();
                var viewModel = new LoginViewModel();

                viewModel.OnSettingsCompleted += OnSettingsCompleted;
                viewModel.OnUserLoginCompleted += OnUserLoginCompleted;

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

            // Set a minimum splash screen duration
            var minSplashTime = Task.Delay(2000); // Ensures at least 2 seconds

            // Wait for both minimum splash time and all startup tasks to finish
            await Task.WhenAll(minSplashTime, initTask, loadGroupsTask);

<<<<<<< HEAD
            
=======
            // Ensure the groups are loaded before checking and toggling the shade
           //  mainViewModel.CheckGroupsAndToggleShade(); // Call after groups are loaded
>>>>>>> a97430dcbddd1584f44d3090e914d1dddb0422a0

            splashScreen.Close(); // Close splash screen once everything is set

            var initResult = await initTask; // Retrieve the initialization result

            if (initResult.openSettings)
            {
                _settingsWindow = new Settings { DataContext = initResult.viewModel };
                _settingsWindow.ShowDialog();

                _userLoginWindow = new UserLogin { DataContext = initResult.viewModel };
                _userLoginWindow.ShowDialog();
            }
            else if (initResult.isUserDataPresent && initResult.isUserLoggedIn)
            {
                OpenMainWindow(mainViewModel);
            }
            else
            {
                _userLoginWindow = new UserLogin { DataContext = initResult.viewModel };
                _userLoginWindow.ShowDialog();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            DisposeNotifyIcon();
            base.OnExit(e);
        }

        public static void DisposeNotifyIcon()
        {
            if (NotifyIconInstance != null)
            {
                NotifyIconInstance.Visible = false;
                NotifyIconInstance.Dispose();
                NotifyIconInstance = null;
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

            // Initialize MainViewModel and load groups before opening MainWindow
            var mainViewModel = new MainViewModel();

            Task.Run(async () =>
            {
                await mainViewModel.LoadUserGroupsAsync();

                // Ensure UI updates on the main thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OpenMainWindow(mainViewModel);
                });
            });
        }


        private void OpenMainWindow(MainViewModel mainViewModel)
        {
            var dbService = new SQLiteDBService();


            // Apply user settings (primary color)
            SetPrimaryColorFromUserSelection(dbService);

            _mainWindow = new MainWindow(mainViewModel);
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Focus();

        }

    }
}
