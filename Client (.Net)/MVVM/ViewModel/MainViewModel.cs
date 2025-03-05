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
using Client__.Net_.MVVM.ViewModel;

namespace Chat_App.MVVM.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
       
        private System.Timers.Timer _pollingTimer;
        private  SupabaseService _supabaseService;
        private readonly SQLiteDBService _sqliteDBService;

        // Properties
        public SupabaseSettings SupabaseSettings { get; set; }
        public User User { get; set; }
        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();
        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();
        public ObservableCollection<Group> Groups { get; set; } = new ObservableCollection<Group>();
        public NewGroupViewModel NewGroupViewModel { get; set; }

        // Commands
        private ICommand _sendMessageCommand;
        private ICommand _loadMessagesCommand;
        private ICommand _openUserProfileEditCommand;
        private ICommand _openUserProfileAddCommand;
        private ICommand _openSettingsCommand;
        private ICommand _openAddGroupCommand;



        public ICommand SendMessageCommand => _sendMessageCommand;
        public ICommand LoadMessagesCommand => _loadMessagesCommand;
        public ICommand OpenUserProfileEditCommand => _openUserProfileEditCommand;
        public ICommand OpenUserProfileAddCommand => _openUserProfileAddCommand;
        public ICommand OpenSettingsCommand => _openSettingsCommand;
        public ICommand OpenAddGroupCommand => _openAddGroupCommand;

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
   

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
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
            _sqliteDBService.InitializeDatabase();

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
            NewGroupViewModel = new NewGroupViewModel();
        }

        public async Task InitializeDatabaseAsync()
        {
             await _supabaseService.InitializeDatabaseSchemaAsync();
        }
       

        private void InitializeCommands()
        {
            _sendMessageCommand = new RelayCommand(async _ => await SendMessageAsync(), _ => !string.IsNullOrEmpty(Message));
            _loadMessagesCommand = new RelayCommand(async _ => await LoadMessagesAsync());
            _openSettingsCommand = new RelayCommand(_ => OpenSettings());
            _openUserProfileEditCommand = new RelayCommand(_ => OpenUserProfileEdit());
            _openUserProfileAddCommand = new RelayCommand(_ => OpenUserProfileAdd());
            _openAddGroupCommand = new RelayCommand(OpenAddGroup);
        }

        private void OpenAddGroup(object parameter)
        {
            //Open the AddGroup window


            Groups.Add(new Group
            {
                //Id = 1,
                GroupName = "Group 1",
                Messages = Messages.DefaultIfEmpty(new Message { message = "Hi" }).Last().message,
                ImageSource = "https://img.freepik.com/free-photo/people-posing-together-registration-day_23-2149096794.jpg"
            });
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
