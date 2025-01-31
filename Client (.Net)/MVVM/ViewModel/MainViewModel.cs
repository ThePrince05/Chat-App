using Chat_App.MVVM.Core;
using Chat_App.MVVM.Model;
using Chat_App.Net;
using Client__.Net_.MVVM.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Windows.Navigation;
using Client__.Net_.MVVM.View;
using Client__.Net_;
using System.Collections.Specialized;


namespace Chat_App.MVVM.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
       
        private System.Timers.Timer _pollingTimer;
        private  SupabaseService _supabaseService;
        private Server _server;
        private readonly SQLiteDBService _sqliteDBService;

        // Properties
        public ServerSettings ServerSettings { get; set; }
        public SupabaseSettings SupabaseSettings { get; set; }
        public UserModel User { get; set; }
        public ObservableCollection<UserModel> Users { get; } = new ObservableCollection<UserModel>();
        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

        // Commands
        private ICommand _finishSettingsCommand;
        private ICommand _connectToServerCommand;
        private ICommand _sendMessageCommand;
        private ICommand _loadMessagesCommand;
        private ICommand _loginCommand;
        private ICommand _nextSettingsCommand;
        private ICommand _openUserProfileCommand;
        private ICommand _openSettingsCommand;

        public ICommand FinishSettingsCommand => _finishSettingsCommand;
        public ICommand SendMessageCommand => _sendMessageCommand;
        public ICommand LoadMessagesCommand => _loadMessagesCommand;
        public ICommand LoginCommand => _loginCommand;
        public ICommand NextSettingsCommand => _nextSettingsCommand;
        public ICommand OpenUserProfileCommand => _openUserProfileCommand;
        public ICommand OpenSettingsCommand => _openSettingsCommand;

        // Events
        public event EventHandler ProfileCompleted;
        public event EventHandler SettingsCompleted;

        // Properties for binding
        private string username;
        public string Username
        {
            get => User?.Username; // Reflect the value from UserModel
            set
            {
                if (User != null)
                {
                    User.Username = value; // Update the UserModel
                    OnPropertyChanged(nameof(Username)); // Notify UI of the change
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

        // Property to access IsDedicatedServerEnabled from ServerSettings
        public bool IsDedicatedServerEnabled
        {
            get => ServerSettings.IsDedicatedServerEnabled;
            set
            {
                if (ServerSettings.IsDedicatedServerEnabled != value)
                {
                    ServerSettings.IsDedicatedServerEnabled = value;
                    OnPropertyChanged(nameof(IsDedicatedServerEnabled));
                }
            }
        }

        private SolidColorBrush selectedColor;
        public SolidColorBrush SelectedColor
        {
            get => selectedColor;
            set
            {
                if (SetProperty(ref selectedColor, value))
                {
                    if (User != null)
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

            LoadUserData();

            // Call method to handle the server initialization logic based on conditions
            InitializeServerServices();

        }

        private void InitializeCommands()
        {
            _sendMessageCommand = new RelayCommand(async _ => await SendMessageAsync(), _ => !string.IsNullOrEmpty(Message));
            _loadMessagesCommand = new RelayCommand(async _ => await LoadMessagesAsync());
            _loginCommand = new RelayCommand(async _ => await LogInUser(), _ => !string.IsNullOrEmpty(Username));
            _finishSettingsCommand = new RelayCommand(ExecuteFinishSettings, CanExecuteFinishSettings);
            _nextSettingsCommand = new RelayCommand(ExecuteNextSettings, CanExecuteNextSettings);
            _openSettingsCommand = new RelayCommand(_ => OpenSettings());
        }

        private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ScrollToLastMessage();
                });
            }
        }

        public void ScrollToLastMessage()
        {
            if (Messages.Count > 0)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var listView = Application.Current.MainWindow.FindName("lvMessageList") as System.Windows.Controls.ListView;
                    listView?.ScrollIntoView(Messages[^1]); // Scroll to last item
                });
            }
        }


        internal void OpenUserProfile()
        {
            // Instantiate and show the UserProfile window
            UserProfile userProfileWindow = new UserProfile();
            userProfileWindow.Show();
        }

        private void OpenSettings()
        {
            // Open the Settings window
            var settingsWindow = new Settings();
            settingsWindow.Show();
        }

        private void InitializeServicesAndEvents()
        {
            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectEvent += RemoveUser;
        }

        private void InitializePolling()
        {
            _pollingTimer = new System.Timers.Timer(5000); // Interval in milliseconds
            _pollingTimer.Elapsed += async (sender, e) => await PollMessagesAsync();
            _pollingTimer.AutoReset = true; // Repeat the timer
            _pollingTimer.Enabled = true;   // Start the timer
        }

        private async void InitializeServerServices()
        {
            if (IsDedicatedServerEnabled)
            {
                try
                {
                    _server = new Server(ServerSettings.ServerIp, Convert.ToInt32(ServerSettings.ServerPort));
                    await ConnectToServer();

                    // Initialize Services and Events
                    InitializeServicesAndEvents();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Oops, something went wrong on: {ex.Message}");
                }
            }
        }
        private void LoadUserData()
        {
            User = _sqliteDBService.LoadUser();
            OnPropertyChanged(nameof(User));
        }

        private void LoadSettings()
        {
            var settingsService = new SQLiteDBService();
            var (serverSettings, supabaseSettings) = settingsService.LoadSettings();

            ServerSettings = serverSettings ?? new ServerSettings();
            SupabaseSettings = supabaseSettings ?? new SupabaseSettings();

            // Validate Supabase settings
            if (!string.IsNullOrEmpty(SupabaseSettings.SupabaseUrl) &&
                Uri.TryCreate(SupabaseSettings.SupabaseUrl, UriKind.Absolute, out _))
            {
                InitializeSupabaseService();
            }
        }

        private void InitializeSupabaseService()
        {
            // Initialize SupabaseService only after valid settings are loaded
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

            string selectedColorHex = SelectedColor.Color.ToString(); // Hex format
            _sqliteDBService.SaveUser(Username, selectedColorHex);
          
            MessageBox.Show("User logged in successfully.");
            ProfileCompleted?.Invoke(this, EventArgs.Empty);
        }

        private async Task ConnectToServer()
        {
            _server.ConnectToServer(User.Username);
        }

        private async Task SendMessageAsync()
        {
            if (string.IsNullOrEmpty(Message)) return;

            var timestamp = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");

            if (IsDedicatedServerEnabled)
            {
                _server.SendMessageToServer(Message);
            }

            // Save message to Supabase
            bool isSaved = await _supabaseService.SaveMessageAsync(Username, Message, timestamp);

            if (isSaved)
            {
                //MessageBox.Show("Message saved successfully.");
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
                    // Update messages collection on UI thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Messages.Clear();
                        foreach (var msg in messages)
                        {
                            // Add messages as Message objects
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

        private void UserConnected()
        {
            if (_server.PacketReader == null) return;

            var user = new UserModel
            {
                Username = _server.PacketReader.ReadMessage(),
                UID = _server.PacketReader.ReadMessage()
            };

            if (!Users.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }
        }

        private void MessageReceived()
        {
            if (_server.PacketReader == null) return;

            var msg = new Message
            {
                message = _server.PacketReader.ReadMessage(),
                username = _server.PacketReader.ReadMessage(),
                timestamp = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss")
            };

            Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
        }

        private void RemoveUser()
        {
            if (_server.PacketReader == null) return;

            var uid = _server.PacketReader.ReadMessage();
            var user = Users.FirstOrDefault(x => x.UID == uid);
            if (user != null)
            {
                Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
            }
        }

        private void ExecuteNextSettings(object parameter)
        {
            if (!SupabaseSettings.ValidateSupabaseSettings())
            {
                MessageBox.Show("Please fill in all Supabase fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!ServerSettings.ValidateServerSettings())
            {
                MessageBox.Show("Please fill in all server fields when the dedicated server is enabled.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_selectedTabIndex < 1)
            {
                SelectedTabIndex++;
            }
            else
            {
                MessageBox.Show("You're already on the last tab.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanExecuteNextSettings(object parameter) =>
            SupabaseSettings.ValidateSupabaseSettings() && ServerSettings.ValidateServerSettings();

        private void ExecuteFinishSettings(object parameter)
        {
            if (!SupabaseSettings.ValidateSupabaseSettings())
            {
                MessageBox.Show("Please fill in all Supabase fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!ServerSettings.ValidateServerSettings())
            {
                MessageBox.Show("Please fill in all server fields when the dedicated server is enabled.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _sqliteDBService.SaveSettings(
                SupabaseSettings.SupabaseUrl,
                SupabaseSettings.SupabaseApiKey,
                IsDedicatedServerEnabled,
                ServerSettings.ServerIp,
                Convert.ToInt32(ServerSettings.ServerPort)
            );

            MessageBox.Show("Settings saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            SettingsCompleted?.Invoke(this, EventArgs.Empty);
        }

        private bool CanExecuteFinishSettings(object parameter) => true; // Always enabled

        public void Dispose()
        {
            _server.connectedEvent -= UserConnected;
            _server.msgReceivedEvent -= MessageReceived;
            _server.userDisconnectEvent -= RemoveUser;
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



