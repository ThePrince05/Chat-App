using Chat_App.MVVM.Core;
using Chat_App.MVVM.Model;
using Client__.Net_.MVVM.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Collections.Specialized;
using Client__.Net_.MVVM.View;
using Client__.Net_.MVVM.Helpers;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace Chat_App.MVVM.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private System.Timers.Timer _pollingTimer;
        private SupabaseService _supabaseService;
        private readonly SQLiteDBService _sqliteDBService;

        // Properties
        public SupabaseSettings SupabaseSettings { get; set; }
        public User User { get; set; }
        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();
        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();

        // Commands
        private ICommand _sendMessageCommand;
        private ICommand _loadMessagesCommand;
        private ICommand _loginCommand;
        private ICommand _openUserProfileEditCommand;
        private ICommand _openUserProfileAddCommand;
        private ICommand _openSettingsCommand;
        private ICommand _saveSettingsCommand;
        private ICommand _saveUserCommand;
        private ICommand _modifyUserCommand;
        private ICommand _logoutCommand;



        public ICommand SendMessageCommand => _sendMessageCommand;
        public ICommand LoadMessagesCommand => _loadMessagesCommand;
        public ICommand LoginCommand => _loginCommand;
        public ICommand OpenUserProfileEditCommand => _openUserProfileEditCommand;
        public ICommand OpenUserProfileAddCommand => _openUserProfileAddCommand;
        public ICommand OpenSettingsCommand => _openSettingsCommand;
        public ICommand SaveSettingsCommand => _saveSettingsCommand;
        public ICommand SaveUserCommand => _saveUserCommand;
        public ICommand ModifyUserCommand => _modifyUserCommand;
        public ICommand LogoutCommand => _logoutCommand;

        // Events
        public event EventHandler OnUserLoginCompleted;
        public event EventHandler OnSettingsCompleted;

        // Properties for binding
        public string Username
        {
            get => User?.Username;
            set
            {
                if (User != null)
                {
                    User.Username = value;
                    OnPropertyChanged(nameof(Username));
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

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
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
        public MainViewModel()
        {
            Messages.CollectionChanged += Messages_CollectionChanged;
            _sqliteDBService = new SQLiteDBService();

          
            // Initialize Commands
            InitializeCommands();

            // Initialize Polling
            InitializePolling();

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
            _sendMessageCommand = new RelayCommand(async _ => await SendMessageAsync(), _ => !string.IsNullOrEmpty(Message));
            _loadMessagesCommand = new RelayCommand(async _ => await LoadMessagesAsync());
            _loginCommand = new RelayCommand(async _ => await LogInUser(), _ => !string.IsNullOrEmpty(Username));
            _openSettingsCommand = new RelayCommand(_ => OpenSettings());
            _saveSettingsCommand = new RelayCommand(_ => ExecuteSaveSettings());
            _openUserProfileEditCommand = new RelayCommand(_ => OpenUserProfileEdit());
            _openUserProfileAddCommand = new RelayCommand(_ => OpenUserProfileAdd());
            _saveUserCommand = new RelayCommand(async _ => await SaveUserAsync(), _ => CanSaveUser());
            _modifyUserCommand = new RelayCommand(async _ => await ModifyUserAsync(), _ => CanSaveUser());
            _logoutCommand = new RelayCommand(_ =>  ExecuteLogout());
        }
        private void ExecuteLogout()
        {
            var result = MessageBox.Show("Are you sure you want to log out?",
                                         "Confirm Logout",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Step 1: Delete all user logins from SQLite
                _sqliteDBService.DeleteAllUserLogins();

                // Step 2: Get the correct application executable path
                string exePath = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".exe");

                if (File.Exists(exePath))  // Ensure the .exe file exists
                {
                    // Step 3: Start the application again
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath, // Correct executable path
                        UseShellExecute = true,
                    });

                    // Step 4: Exit the current application
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

        private async Task SaveUserAsync()
        {
            if (!CanSaveUser())
            {
                MessageBox.Show("Please fill in all fields!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Encrypt the password using EncryptionHelper
                string encryptedPassword = EncryptionHelper.Encrypt(Password);

                // Check if the SelectedColor is null and use green (#00FF00) as the default
                string selectedColorHex = SelectedColorHex ?? "#00FF00"; // Default to green if null

                // Save to Supabase with the encrypted password
                bool isSaved = await _supabaseService.InsertUserAsync(Username, encryptedPassword, selectedColorHex);

                if (isSaved)
                {
                    // Save to SQLite (consider if password should be stored here, usually not recommended)
                    _sqliteDBService.SaveUser(Username, selectedColorHex);

                    MessageBox.Show("User saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (!CanSaveUser())
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
                bool isUserInSQLite = _sqliteDBService.CheckUserExists(Username);
                if (!isUserInSQLite)
                {
                    MessageBox.Show("User not found in local database!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Step 2: Check if user exists in Supabase
                var userFromSupabase = await _supabaseService.GetUserByUsernameAsync(Username);
                if (userFromSupabase == null)
                {
                    MessageBox.Show("User not found in Supabase!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Step 3: Update the user's details in Supabase
                bool isUpdatedInSupabase = await _supabaseService.UpdateUserAsync(Username, encryptedPassword, selectedColorHex);
                if (!isUpdatedInSupabase)
                {
                    MessageBox.Show("Failed to update user in Supabase!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Step 4: Update the user's details in SQLite
                bool isUpdatedInSQLite = _sqliteDBService.UpdateUser(Username, selectedColorHex);
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

        private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0)
            {
                Application.Current.Dispatcher.InvokeAsync(ScrollToLastMessage);
            }
        }

        private bool _hasScrolledToBottom = false; // Flag to track if scrolling has happened

        public void ScrollToLastMessage()
        {
            if (Messages == null || Messages.Count == 0)
            {
                Debug.WriteLine("Messages collection is null or empty. Skipping scroll.");
                return;
            }

            // If already scrolled once, do nothing
            if (_hasScrolledToBottom)
            {
                Debug.WriteLine("Already scrolled to the bottom once. Skipping scroll.");
                return;
            }

            // Ensure application and dispatcher are available
            if (Application.Current?.Dispatcher == null)
            {
                Debug.WriteLine("Application or Dispatcher is null. Skipping scroll.");
                return;
            }

            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow == null)
                {
                    Debug.WriteLine("MainWindow is null. Skipping scroll.");
                    return;
                }

                var listView = mainWindow.FindName("lvMessageList") as System.Windows.Controls.ListView;
                if (listView == null)
                {
                    Debug.WriteLine("ListView 'lvMessageList' not found in MainWindow. Skipping scroll.");
                    return;
                }

                // Ensure ListView has at least one item before scrolling
                if (listView.Items.Count > 0)
                {
                    listView.ScrollIntoView(Messages[^1]);
                    _hasScrolledToBottom = true; // Set flag to prevent future scrolling
                    Debug.WriteLine("Scrolled to the last message.");
                }
                else
                {
                    Debug.WriteLine("ListView is empty. Skipping scroll.");
                }
            });
        }



        internal static void OpenUserProfileEdit()
        {
            UserProfileEdit userProfileEditWindow = new();
            userProfileEditWindow.ShowDialog();
        }
        internal static void OpenUserProfileAdd()
        {
            UserProfile userProfileAddWindow = new UserProfile();
            userProfileAddWindow.ShowDialog();
        }

        private static void OpenSettings()
        {
            var settingsWindow = new Settings();
            settingsWindow.ShowDialog();
        }

        private void InitializePolling()
        {
            _pollingTimer = new System.Timers.Timer(5000);
            _pollingTimer.Elapsed += async (sender, e) => await PollMessagesAsync();
            _pollingTimer.AutoReset = true;
            _pollingTimer.Enabled = true;
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
                    MessageBox.Show("Username not found in local database!", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Step 2: Check if the username exists in Supabase
                var userFromSupabase = await _supabaseService.GetUserByUsernameAsync(Username);

                if (userFromSupabase == null)
                {
                    MessageBox.Show("Username not found in Supabase!", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Step 3: Decrypt the stored password and compare with entered password
                string decryptedPassword;
                try
                {
                    decryptedPassword = EncryptionHelper.Decrypt(userFromSupabase.UserPassword);
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

                // Step 4: Insert a new UserLogin record with status set to true
                _sqliteDBService.InsertUserLoginStatus(Username, true);

                // Step 5: Hide the current login window
                if (Application.Current.Windows.OfType<UserLogin>().FirstOrDefault() is UserLogin loginWindow)
                {
                    loginWindow.Hide();
                }

                // Step 6: Open the MainWindow
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during login: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private async Task SendMessageAsync()
        {
            if (string.IsNullOrEmpty(Message)) return;

            var timestamp = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");

            bool isSaved = await _supabaseService.SaveMessageAsync(Username, Message, timestamp);

            if (isSaved)
            {
                await LoadMessagesAsync();
            }
            else
            {
                MessageBox.Show("Failed to save message.");
            }

            Message = string.Empty;
        }

        private async Task PollMessagesAsync()
        {
            if (_supabaseService != null)
            {
                await LoadMessagesAsync();
            }
            else
            {
                Console.WriteLine("SupabaseService not initialized. Messages will not be loaded.");
            }
        }

        private async Task LoadMessagesAsync()
        {
            if (_supabaseService == null)
            {
                Console.WriteLine("SupabaseService is not initialized. Cannot load messages.");
                return;
            }

            try
            {
                Console.WriteLine("Fetching messages from Supabase...");
                var messages = await _supabaseService.GetMessagesAsync();

                if (messages == null || !messages.Any())  // Check for null or empty list
                {
                    Console.WriteLine("No messages found or received null response.");
                    return;
                }

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    if (Application.Current == null) return; // Ensure app is still running

                    if (Messages == null)
                    {
                        Messages = new ObservableCollection<Message>();
                    }

                    Messages.Clear();

                    foreach (var msg in messages)
                    {
                        if (msg != null)
                        {
                            Messages.Add(msg);
                        }
                    }

                    Console.WriteLine($"{Messages.Count} messages loaded.");
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading messages: {ex.Message}");
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
        public void Dispose()
        {
            _pollingTimer?.Dispose();
        }

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
