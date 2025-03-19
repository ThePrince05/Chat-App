
using Client__.Net_.MVVM.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Collections.Specialized;
using Client__.Net_.MVVM.View;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using Client__.Net_.Core;
using Chat_App.Core.Model;
using System.Net.NetworkInformation;
using System.Net.Http;

namespace Client__.Net_.MVVM.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {

        private System.Timers.Timer _pollingTimer;
        private SupabaseService _supabaseService;
        private readonly SQLiteDBService _sqliteDBService;
        private readonly HttpClient _httpClient = new HttpClient();
        private bool _isConnected = false;

        // Properties
        public SupabaseSettings SupabaseSettings { get; set; }
        public User User { get; set; }
        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();
        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();
        public ObservableCollection<Group> Groups { get; set; } = new ObservableCollection<Group>();
        public NewGroupViewModel NewGroupViewModel { get; set; }

        // Commands
        private ICommand _sendMessageCommand;
        private ICommand _openUserProfileEditCommand;
        private ICommand _openUserProfileAddCommand;
        private ICommand _openSettingsCommand;


        public ICommand SendMessageCommand => _sendMessageCommand;
        public ICommand OpenUserProfileEditCommand => _openUserProfileEditCommand;
        public ICommand OpenUserProfileAddCommand => _openUserProfileAddCommand;
        public ICommand OpenSettingsCommand => _openSettingsCommand;
 

        // Events
        public event EventHandler OnUserLoginCompleted;
        public event EventHandler OnSettingsCompleted;
        
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

        public string SelectedColor
        {
            get => User?.SelectedColor;
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
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));

                Debug.WriteLine($"Message updated: {_message}");

                // Update the SendMessageCommand state
                (_sendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }



        private Group _selectedGroup;
        public Group SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                _selectedGroup = value;
                OnPropertyChanged(nameof(SelectedGroup));
                Debug.WriteLine(_selectedGroup != null
                    ? $"Selected Group set: {_selectedGroup.GroupName}"
                    : "Selected Group set to null.");
                // Notify the command that its state might have changed
                (_sendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }


        // Constructor
        public MainViewModel()
        {

            _sqliteDBService = new SQLiteDBService();
            _sqliteDBService.InitializeDatabase();

            // Initialize Polling
            InitializePolling();

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

            NewGroupViewModel = new NewGroupViewModel(this, SupabaseSettings);

            // Start the connection check (polling every 5 seconds)
            StartConnectionCheck();


            InitializePolling();

        }

        public event Action ToggleNewGroupPanel;
        public void TogglePanel()
        {
            ToggleNewGroupPanel?.Invoke();
        }
        public async Task InitializeDatabaseAsync()
        {
            await _supabaseService.InitializeDatabaseSchemaAsync();
        }


        private void InitializeCommands()
        {
            _sendMessageCommand = new RelayCommand(
            async _ =>
                    {
                        if (SelectedGroup != null)
                        {
                            Debug.WriteLine($"Executing SendMessageAsync for Group ID: {SelectedGroup.Id}");
                            await SendMessageAsync(SelectedGroup.Id);
                        }
                        else
                        {
                            Debug.WriteLine("SendMessageAsync: No group selected.");
                            MessageBox.Show("Please select a group before sending a message.");
                        }
                    },
                    _ => !string.IsNullOrEmpty(Message) && SelectedGroup != null
                );

            
            _openSettingsCommand = new RelayCommand(_ => OpenSettings());
            _openUserProfileEditCommand = new RelayCommand(_ => OpenUserProfileEdit());
            _openUserProfileAddCommand = new RelayCommand(_ => OpenUserProfileAdd());
            
        }

        // these ping google to check internet for loadusergroups
        private async void StartConnectionCheck()
        {
            while (true) // Loop to check connection status periodically
            {
                if (await IsInternetAvailable())
                {
                    if (!_isConnected)
                    {
                        _isConnected = true; // Mark as connected
                        Debug.WriteLine("Internet is back online. Reloading groups...");
                        LoadUserGroupsAsync(); // Reload groups when reconnected
                    }
                }
                else
                {
                    _isConnected = false; // Mark as disconnected
                    Debug.WriteLine("No internet connection detected.");
                }

                await Task.Delay(5000); // Check every 5 seconds
            }
        }

        private async Task<bool> IsInternetAvailable()
        {
            try
            {
                // Attempt to ping a reliable online service (can be replaced with your own endpoint)
                var response = await _httpClient.GetAsync("https://www.google.com");
                return response.IsSuccessStatusCode; // Return true if the service is reachable
            }
            catch
            {
                return false; // Return false if an exception occurs (e.g., no internet)
            }
        }
        public async void LoadUserGroupsAsync()
        {
            string currentUsername = User.Username; // Ensure you have a way to access the current username
            var userGroups = await _supabaseService.GetUserGroupsAsync(currentUsername);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Groups.Clear();
                foreach (var group in userGroups)
                {
                    Groups.Add(group);
                }
            });
        }

   

        private int? _lastOpenedGroupId = null; // ✅ Keeps track of the last opened group

        public void ScrollToLastMessage(int groupId)
        {
            if (Messages == null || Messages.Count == 0)
            {
                Debug.WriteLine("Messages collection is null or empty. Skipping scroll.");
                return;
            }

            // ✅ Only scroll if the user switches to a new group
            if (_lastOpenedGroupId == groupId)
            {
                Debug.WriteLine($"Already opened Group ID {groupId}. Skipping scroll.");
                return;
            }

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

                if (listView.Items.Count > 0)
                {
                    listView.ScrollIntoView(Messages[^1]); // ✅ Scroll to the last message
                    _lastOpenedGroupId = groupId; // ✅ Mark this group as viewed
                    Debug.WriteLine($"Scrolled to last message for Group ID {groupId}.");
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

            // Load user theme
            App.SetPrimaryColorFromUserSelection(_sqliteDBService);
        }

        private void InitializeSupabaseService()
        {
            _supabaseService = new SupabaseService(SupabaseSettings);
            Debug.WriteLine("SupabaseService initialized successfully.");
        }


        private async Task SendMessageAsync(int groupId)
        {
            if (string.IsNullOrEmpty(Message))
            {
                Debug.WriteLine("SendMessageAsync: Message is empty, exiting.");
                return;
            }

            Debug.WriteLine($"SendMessageAsync: Sending message to group ID {groupId}");

            bool isSaved = await _supabaseService.SaveMessageAsync(Username, Message, groupId);

            if (isSaved)
            {
                Debug.WriteLine("SendMessageAsync: Message sent successfully!");
                await LoadMessagesAsync(groupId); // Refresh messages for the same group
            }
            else
            {
                Debug.WriteLine("SendMessageAsync: Failed to save message.");
                MessageBox.Show("Failed to save message.");
            }

            Message = string.Empty;
            Debug.WriteLine("SendMessageAsync: Message input cleared.");

            _lastOpenedGroupId = null;
            ScrollToLastMessage(SelectedGroup.Id);
        }


        private async void InitializePolling()
        {
            // Fetch messages immediately
            await PollMessagesAsync();

            // Set up the timer for periodic polling
            _pollingTimer = new System.Timers.Timer(5000);
            _pollingTimer.Elapsed += async (sender, e) => await PollMessagesAsync();
            _pollingTimer.AutoReset = true;
            _pollingTimer.Enabled = true;
        }

        private async Task PollMessagesAsync()
        {
            if (_supabaseService == null)
            {
                Debug.WriteLine("SupabaseService not initialized. Messages will not be loaded.");
                return;
            }

            if (SelectedGroup == null)
            {
                Debug.WriteLine("No group selected. Skipping message polling.");
                return;
            }

            Debug.WriteLine($"Polling messages for Group ID: {SelectedGroup.Id}");
            await LoadMessagesAsync(SelectedGroup.Id);
        }

        public async Task LoadMessagesAsync(int groupId)
        {
            if (_supabaseService == null)
            {
                Debug.WriteLine("SupabaseService is not initialized. Cannot load messages.");
                return;
            }

            try
            {
                Debug.WriteLine($"Fetching messages for Group ID {groupId}...");
                var messages = await _supabaseService.GetMessagesAsync(groupId);

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    if (Application.Current == null) return;

                    Messages ??= new ObservableCollection<Message>();
                    Messages.Clear(); // ✅ Always clear previous messages

                    if (messages != null && messages.Any())
                    {
                        foreach (var msg in messages)
                        {
                            Messages.Add(msg);
                        }

                        Debug.WriteLine($"{Messages.Count} messages loaded.");
                        ScrollToLastMessage(SelectedGroup.Id); // ✅ Scroll after loading messages
                    }
                    else
                    {
                        Debug.WriteLine("No messages found. Clearing message list.");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading messages: {ex.Message}");
            }
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
