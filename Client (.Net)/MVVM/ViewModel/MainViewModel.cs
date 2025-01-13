using Chat_App.MVVM.Core;
using Chat_App.MVVM.Model;
using Chat_App.Net;
using Client__.Net_.MVVM.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Timers;

namespace Chat_App.MVVM.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private System.Timers.Timer _pollingTimer;
        private readonly SupabaseService _supabaseService;
        private readonly Server _server;
        private readonly DatabaseService _databaseService;
        private SolidColorBrush selectedColor;

        // ObservableCollections for binding
        public ObservableCollection<UserModel> Users { get; } = new ObservableCollection<UserModel>();
        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

        // Commands
        public ICommand ConnectToServerCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand LoadMessagesCommand { get; }
        public ICommand LoginCommand { get; }

        private string username;
        public string Username
        {
            get => username;
            set
            {
                if (username != value)
                {
                    username = value;
                    OnPropertyChanged(nameof(Username));

                    // If CurrentUser is not null, update the Username in UserModel
                    if (CurrentUser != null)
                    {
                        CurrentUser.Username = value;
                    }
                }
            }
        }

        public UserModel CurrentUser { get; set; }  // Keep this as a single declaration

        private string _connectedUsername;
        public string ConnectedUsername
        {
            get => _connectedUsername;
            set => SetProperty(ref _connectedUsername, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        // Constructor
        public MainViewModel()
        {
            _server = new Server();
            _supabaseService = new SupabaseService();
            _databaseService = new DatabaseService();

            // Initialize Commands
            ConnectToServerCommand = new RelayCommand(async _ => await ConnectToServer(), _ => !string.IsNullOrEmpty(Username));
            SendMessageCommand = new RelayCommand(async _ => await SendMessageAsync(), _ => !string.IsNullOrEmpty(Message));
            LoadMessagesCommand = new RelayCommand(async _ => await LoadMessagesAsync());

            // Initialize Commands
            LoginCommand = new RelayCommand(async _ => await LogInUser(), _ => !string.IsNullOrEmpty(Username));

            // Initialize Services and Events
            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectEvent += RemoveUser;

            // Initialize Polling
            InitializePolling();

            // Initialize Database and Load Messages
            InitializeDatabase();
            _ = LoadMessagesAsync();

            // Initialize SelectedColor to a default color, e.g., Green
            SelectedColor = new SolidColorBrush(Colors.Green);
        }

        private async Task LogInUser()
        {
            if (string.IsNullOrEmpty(Username))
            {
                MessageBox.Show("Username is required.");
                return;
            }

            // Get the SelectedColor as a string (You can use any color format, e.g., Hex)
            string selectedColorHex = SelectedColor.Color.ToString(); // For example, #FF00FF

            // Save the user to the database
            _databaseService.SaveUser(Username, selectedColorHex);

            // You can also do other actions after login like navigating to the chat screen or setting a flag
            MessageBox.Show("User logged in successfully.");
        }

        public SolidColorBrush SelectedColor
        {
            get => selectedColor;
            set
            {
                selectedColor = value;
                OnPropertyChanged(nameof(SelectedColor));

                // Link to CurrentUser's SelectedColor (UserModel)
                if (CurrentUser != null)
                {
                    CurrentUser.SelectedColor = value;
                }
            }
        }

        private async Task ConnectToServer()
        {
            ConnectedUsername = Username;
            Username = string.Empty;
            _server.ConnectToServer(ConnectedUsername);
        }

        private async Task SendMessageAsync()
        {
            if (string.IsNullOrEmpty(Message)) return;

            var timestamp = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");
            _server.SendMessageToServer(Message);

            if (await _supabaseService.SaveMessageAsync(ConnectedUsername, Message, timestamp))
            {
                await LoadMessagesAsync();
            }

            Message = string.Empty;
        }

        private async Task PollMessagesAsync()
        {
            await LoadMessagesAsync();
        }

        private void InitializePolling()
        {
            _pollingTimer = new System.Timers.Timer(5000); // Interval in milliseconds
            _pollingTimer.Elapsed += async (sender, e) => await PollMessagesAsync();
            _pollingTimer.AutoReset = true; // Repeat the timer
            _pollingTimer.Enabled = true;   // Start the timer
        }

        private async Task LoadMessagesAsync()
        {
            try
            {
                var messages = await _supabaseService.GetMessagesAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Clear();
                    foreach (var msg in messages)
                        Messages.Add(msg);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading messages: {ex.Message}");
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                _databaseService.InitializeDatabase();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
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
