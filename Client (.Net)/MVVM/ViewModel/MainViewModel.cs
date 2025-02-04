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

namespace Chat_App.MVVM.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private System.Timers.Timer _pollingTimer;
        private SupabaseService _supabaseService;
        private readonly SQLiteDBService _sqliteDBService;

        // Properties
        public SupabaseSettings SupabaseSettings { get; set; }
        public UserModel User { get; set; }
        public ObservableCollection<UserModel> Users { get; } = new ObservableCollection<UserModel>();
        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

        // Commands
        private ICommand _sendMessageCommand;
        private ICommand _loadMessagesCommand;
        private ICommand _loginCommand;
        private ICommand _nextSettingsCommand;
        private ICommand _openUserProfileCommand;
        private ICommand _openSettingsCommand;
        private ICommand _saveSettingsCommand;

        public ICommand SendMessageCommand => _sendMessageCommand;
        public ICommand LoadMessagesCommand => _loadMessagesCommand;
        public ICommand LoginCommand => _loginCommand;
        public ICommand NextSettingsCommand => _nextSettingsCommand;
        public ICommand OpenUserProfileCommand => _openUserProfileCommand;
        public ICommand OpenSettingsCommand => _openSettingsCommand;
        public ICommand SaveSettingsCommand => _saveSettingsCommand;

        // Events
        public event EventHandler ProfileCompleted;
        public event EventHandler SettingsCompleted;

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

        private SolidColorBrush selectedColor;
        public SolidColorBrush SelectedColor
        {
            get => selectedColor;
            set
            {
                if (SetProperty(ref selectedColor, value) && User != null)
                {
                    User.SelectedColor = value;
                }
            }
        }

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

            LoadUserData();
        }

        private void InitializeCommands()
        {
            _sendMessageCommand = new RelayCommand(async _ => await SendMessageAsync(), _ => !string.IsNullOrEmpty(Message));
            _loadMessagesCommand = new RelayCommand(async _ => await LoadMessagesAsync());
            _loginCommand = new RelayCommand(async _ => await LogInUser(), _ => !string.IsNullOrEmpty(Username));
            _openSettingsCommand = new RelayCommand(_ => OpenSettings());
            _saveSettingsCommand = new RelayCommand(_ => ExecuteSaveSettings());
        }

        private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0)
            {
                Application.Current.Dispatcher.InvokeAsync(ScrollToLastMessage);
            }
        }

        public void ScrollToLastMessage()
        {
            if (Messages.Count > 0)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var listView = Application.Current.MainWindow.FindName("lvMessageList") as System.Windows.Controls.ListView;
                    listView?.ScrollIntoView(Messages[^1]);
                });
            }
        }

        internal void OpenUserProfile()
        {
            UserProfile userProfileWindow = new UserProfile();
            userProfileWindow.ShowDialog();
        }

        private void OpenSettings()
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
            if (string.IsNullOrEmpty(Username))
            {
                MessageBox.Show("Username is required.");
                return;
            }

            // Check if the SelectedColor is null and use green (#00FF00) as the default
            string selectedColorHex = SelectedColor?.Color.ToString() ?? "#00FF00"; // Default to green if null

            // Save the user with the selected color
            _sqliteDBService.SaveUser(Username, selectedColorHex);

            MessageBox.Show("User logged in successfully.");
            ProfileCompleted?.Invoke(this, EventArgs.Empty);
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
                var messages = await _supabaseService.GetMessagesAsync();
                if (messages != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Messages.Clear();
                        foreach (var msg in messages)
                        {
                            Messages.Add(msg);
                        }
                    });

                    Console.WriteLine($"{Messages.Count} messages loaded.");
                }
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
            SettingsCompleted?.Invoke(this, EventArgs.Empty);
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
