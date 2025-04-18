using Chat_App.Core;
using Chat_App;
using Client__.Net_.MVVM.Model;
using Client__.Net_.MVVM.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using Chat_App.Core.Model;
using System.IO;
using Client__.Net_.Core;
using Client__.Net_.Helpers;

namespace Client__.Net_.MVVM.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
     
        private SupabaseService _supabaseService;
        private readonly SQLiteDBService _sqliteDBService;

        // Properties
        public SupabaseSettings SupabaseSettings { get; set; }
        public User User { get; set; }
        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();
     
        // Commands
        private ICommand _loginCommand;
        private ICommand _saveSettingsCommand;
        private ICommand _saveUserCommand;
        private ICommand _modifyUserCommand;
        private ICommand _logoutCommand;
        private ICommand _openUserProfileAddCommand;


        public ICommand LoginCommand => _loginCommand;
        public ICommand SaveSettingsCommand => _saveSettingsCommand;
        public ICommand SaveUserCommand => _saveUserCommand;
        public ICommand ModifyUserCommand => _modifyUserCommand;
        public ICommand LogoutCommand => _logoutCommand;
        public ICommand OpenUserProfileAddCommand => _openUserProfileAddCommand;


        // Events
        public event EventHandler OnUserLoginCompleted;
        public event EventHandler OnSettingsCompleted;

        private string _username; // Backing field for the binding

        public string Username
        {
            get => _username; // Get the value from the backing field
            set
            {
                if (_username != value)
                {
                    _username = value; // Set the value to the backing field
                    if (User != null)
                    {
                        User.Username = value; // Also set the value to the User model
                        OnPropertyChanged(nameof(Username)); // Notify binding updates
                    }
                }
            }
        }


        public string Password
        {
            get => User?.UserPassword;
            set
            {
                if (User != null)
                {
                    User.UserPassword = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        private SolidColorBrush _selectedColor = new SolidColorBrush(Colors.Green); // Default to Green
        public SolidColorBrush SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                _selectedColor = value;
                OnPropertyChanged(nameof(SelectedColor));
                OnPropertyChanged(nameof(SelectedColorHex)); // Notify UI
            }
        }

        // Convert to hex string when needed
        public string SelectedColorHex => SelectedColor.Color.ToString();

        // Constructor
        public LoginViewModel()
        {
           
            _sqliteDBService = new SQLiteDBService();
            _sqliteDBService.InitializeDatabase();

            // Initialize Commands
            InitializeCommands();


            // Load settings and user data
            LoadSettings();

            // Initialize SupabaseService and subscribe to connection failed event
            _supabaseService = new SupabaseService(new SupabaseSettings
            {
                SupabaseUrl = SupabaseSettings.SupabaseUrl,
                SupabaseApiKey = SupabaseSettings.SupabaseApiKey
            });

            _supabaseService.OnConnectionFailed += HandleConnectionFailure;

            // Explicitly running the async method in a background thread
            Task.Run(() => InitializeDatabaseAsync());

            LoadUserData();
        
        }

        public async Task InitializeDatabaseAsync()
        {
            await _supabaseService.InitializeDatabaseSchemaAsync();
        }


        private void InitializeCommands()
        {
            _loginCommand = new RelayCommand(async _ => await LogInUser(), _ => !string.IsNullOrEmpty(Username));
            _saveUserCommand = new RelayCommand(async param => await SaveUserAsync(param as Window), _ => CanSaveUser());
            _modifyUserCommand = new RelayCommand(async _ => await ModifyUserAsync(), _ => CanModifyUser());
            _saveSettingsCommand = new RelayCommand(_ => ExecuteSaveSettings());
            _logoutCommand = new RelayCommand(_ => ExecuteLogout());
            _openUserProfileAddCommand = new RelayCommand(_ => OpenUserProfileAdd());
        }

        public void ExecuteLogout()
        {
            var result = MessageBox.Show("Are you sure you want to log out?",
                                         "Confirm Logout",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Step 1: Delete all user logins from SQLite
                _sqliteDBService.DeleteAllUserLogins();

                // Step 2: Dispose NotifyIcon before restarting
                App.DisposeNotifyIcon();

                // Step 3: Get the correct application executable path
                string exePath = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".exe");

                if (File.Exists(exePath)) // Ensure the .exe file exists
                {
                    // Step 4: Start the application again
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true,
                    });

                    // Step 5: Exit the current application
                    Application.Current.Shutdown();
                    Environment.Exit(0);
                }
                else
                {
                    MessageBox.Show($"Error: Could not find the application executable.\nExpected: {exePath}",
                                    "Restart Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }




        private bool CanSaveUser()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   SelectedColor != null;
        }


        private bool CanModifyUser()
        {
            return !string.IsNullOrWhiteSpace(User.Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   SelectedColor != null;
        }

        private async Task SaveUserAsync(Window userProfileAddWindow)
        {
            if (!CanSaveUser())
            {
                MessageBox.Show("Please fill in all fields!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Encrypt the password
                string encryptedPassword = EncryptionHelper.Encrypt(Password);

                // Use green (#00FF00) as default if SelectedColorHex is null
                string selectedColorHex = SelectedColorHex ?? "#00FF00";

                // Save to Supabase with encrypted password
                bool isSaved = await _supabaseService.InsertUserAsync(Username, encryptedPassword, selectedColorHex);

                if (isSaved)
                {
                    // Save to SQLite
                    _sqliteDBService.SaveUser(Username, selectedColorHex);

                    MessageBox.Show("User saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Close the UserProfileAdd window
                    userProfileAddWindow?.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save user to Supabase!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ModifyUserAsync()
        {
            if (!CanModifyUser())
            {
                MessageBox.Show("Please fill in all fields!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Encrypt the new password
                string encryptedPassword = EncryptionHelper.Encrypt(Password);

                // Default to green (#00FF00) if no color is selected
                string selectedColorHex = SelectedColorHex ?? "#00FF00";

                // Step 1: Check if user exists in SQLite
                bool isUserInSQLite = _sqliteDBService.CheckUserExists(User.Username);
                if (!isUserInSQLite)
                {
                    MessageBox.Show("User not found in local database!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Step 2: Check if user exists in Supabase
                var userFromSupabase = await _supabaseService.GetUserByUsernameAsync(User.Username);
                if (userFromSupabase == null)
                {
                    MessageBox.Show("User not found in Supabase!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Step 3: Update the user's details in Supabase
                bool isUpdatedInSupabase = await _supabaseService.UpdateUserAsync(User.Username, encryptedPassword, selectedColorHex);
                if (!isUpdatedInSupabase)
                {
                    MessageBox.Show("Failed to update user in Supabase!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Step 4: Update the user's details in SQLite
                bool isUpdatedInSQLite = _sqliteDBService.UpdateUser(User.Username, selectedColorHex);
                if (!isUpdatedInSQLite)
                {
                    MessageBox.Show("Failed to update user in local database!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Step 5: Show success message
                MessageBox.Show("User updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private static void OpenSettings()
        {
            var settingsWindow = new Settings();
            settingsWindow.ShowDialog();
        }


        internal static void OpenUserProfileAdd()
        {
            UserProfile userProfileAddWindow = new UserProfile();
            userProfileAddWindow.ShowDialog();
        }

        private void LoadUserData()
        {
            User = _sqliteDBService.LoadUser();
            OnPropertyChanged(nameof(User));
        }

        private void LoadSettings()
        {
            var settingsService = new SQLiteDBService();
            var supabaseSettings = settingsService.LoadSettings();

            SupabaseSettings = supabaseSettings ?? new SupabaseSettings();

            if (!string.IsNullOrEmpty(SupabaseSettings.SupabaseUrl) &&
                Uri.TryCreate(SupabaseSettings.SupabaseUrl, UriKind.Absolute, out _))
            {
                InitializeSupabaseService();
            }
        }

        private void InitializeSupabaseService()
        {
            _supabaseService = new SupabaseService(SupabaseSettings);
            Console.WriteLine("SupabaseService initialized successfully.");
        }

        private async Task LogInUser()
        {
            // Load settings and user data
            LoadSettings();

            // Initialize SupabaseService
            _supabaseService = new SupabaseService(SupabaseSettings);

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Please enter both Username and Password!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Step 1: Check if the username exists in SQLite
                bool isUserInSQLite = _sqliteDBService.CheckUserExists(Username);

                if (!isUserInSQLite)
                {
                    // Step 2: If not found, check Supabase for the user
                    var userFromSupabase = await _supabaseService.GetUserByUsernameAsync(Username);

                    if (userFromSupabase == null)
                    {
                        MessageBox.Show("Username not found in Supabase!", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Step 3: Save user in SQLite (only if found in Supabase)
                    _sqliteDBService.SaveUser(userFromSupabase.Username, userFromSupabase.SelectedColor);
                    // Debug.WriteLine("User retrieved from Supabase and saved locally.");
                }

                // Step 4: Retrieve user from Supabase again to validate credentials
                var verifiedUser = await _supabaseService.GetUserByUsernameAsync(Username);

                if (verifiedUser == null)
                {
                    MessageBox.Show("Username not found in Supabase!", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Step 5: Decrypt stored password and compare with entered password
                string decryptedPassword;
                try
                {
                    decryptedPassword = EncryptionHelper.Decrypt(verifiedUser.UserPassword);
                }
                catch (Exception)
                {
                    MessageBox.Show("Error decrypting password!", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (Password != decryptedPassword)
                {
                    MessageBox.Show("Incorrect password!", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Step 6: Insert a new UserLogin record with status set to true
                _sqliteDBService.InsertUserLoginStatus(Username, true);

                // Step 7: Hide the current login window
                if (Application.Current.Windows.OfType<UserLogin>().FirstOrDefault() is UserLogin loginWindow)
                {
                    loginWindow.Hide();
                }

                // Step 8: Create and pass the MainViewModel to MainWindow
                var mainViewModel = new MainViewModel();
                var loadGroupsTask = mainViewModel.LoadUserGroupsAsync(); // Load groups in parallel

                // Wait for the groups to load before opening the MainWindow
                await loadGroupsTask;

                // Initialize MainWindow and pass the ViewModel
                MainWindow mainWindow = new MainWindow(mainViewModel);
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during login: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private async void ExecuteSaveSettings()
        {
            if (!SupabaseSettings.ValidateSupabaseSettings())
            {
                MessageBox.Show("Please fill in all Supabase fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var supabaseService = new SupabaseService(SupabaseSettings);
            bool isValid = await supabaseService.ValidateSupabaseCredentials();

            if (!isValid)
            {
                MessageBox.Show("Invalid Supabase credentials. Please check your URL and API key.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Save settings in SQLite
            _sqliteDBService.SaveSettings(
                SupabaseSettings.SupabaseUrl,
                SupabaseSettings.SupabaseApiKey
            );


            MessageBox.Show("Settings saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            OnSettingsCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void HandleConnectionFailure(string message)
        {
            // Show the settings window with the failure message
            MessageBox.Show(message, "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Optionally, open settings window here
            OpenSettings();
        }

        // INotifyProperty
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
