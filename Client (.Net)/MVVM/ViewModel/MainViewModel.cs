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

namespace Chat_App.MVVM.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        private System.Timers.Timer _pollingTimer;
        private readonly SupabaseService _supabaseService;
        private readonly Server _server;

        // ObservableCollections for binding
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<Message> Messages { get; set; }

        // Commands
        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand LoadMessagesCommand { get; set; }

        // Properties
        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

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
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<Message>();
            _server = new Server();
            _supabaseService = new SupabaseService();

            // Subscribe to events
            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectEvent += RemoveUser;

            // Initialize Commands
            InitializeCommands();

            // Load messages on startup
            _ = LoadMessagesAsync();

            // Initialize polling
            InitializePolling();
        }

        private void InitializeCommands()
        {
            // Connect to server command
            ConnectToServerCommand = new RelayCommand(
                async o =>
                {
                    await ConnectToServer();
                },
                o => !string.IsNullOrEmpty(Username) // Only enabled when username is not empty
            );

            // Send message command
            SendMessageCommand = new RelayCommand(
                async o => await SendMessageAsync(),
                o => !string.IsNullOrEmpty(Message) // Only enabled when message is not empty
            );

            // Load messages command
            LoadMessagesCommand = new RelayCommand(
                async o => await LoadMessagesAsync(),
                o => true // Always enabled
            );
        }

        private async Task ConnectToServer()
        {
            _server.ConnectToServer(Username);
            ConnectedUsername = Username;
            Username = string.Empty;
        }

        private async Task SendMessageAsync()
        {
            if (!string.IsNullOrEmpty(Message))
            {
                var timestamp = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");

                // Send message to server
                _server.SendMessageToServer(Message);

                // Save message to Supabase
                bool isSaved = await _supabaseService.SaveMessageAsync(ConnectedUsername, Message, timestamp);

                if (isSaved)
                {
                    Console.WriteLine("Message saved successfully.");
                    await LoadMessagesAsync();
                }
                else
                {
                    Console.WriteLine("Failed to save message.");
                }

                // Clear the message input
                Message = string.Empty;
            }
        }

        private void InitializePolling()
        {
            _pollingTimer = new System.Timers.Timer(5000);
            _pollingTimer.Elapsed += async (sender, e) => await PollMessagesAsync();
            _pollingTimer.AutoReset = true;
            _pollingTimer.Enabled = true;
        }

        private async Task PollMessagesAsync()
        {
            await LoadMessagesAsync();
        }

        private async Task LoadMessagesAsync()
        {
            try
            {
                Console.WriteLine("Loading messages from Supabase...");

                var messages = await _supabaseService.GetMessagesAsync();

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

                Console.WriteLine("Messages loaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading messages: {ex.Message}");
            }
        }

        private void RemoveUser()
        {
            if (_server.PacketReader != null)
            {
                var uid = _server.PacketReader.ReadMessage();
                var user = Users.FirstOrDefault(x => x.UID == uid);
                if (user != null)
                {
                    Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
                }
            }
        }

        private void MessageReceived()
        {
            if (_server.PacketReader != null)
            {
                // Assuming the PacketReader returns a message string, username, and timestamp.
                var messageContent = _server.PacketReader.ReadMessage();  // Message content (text)
                var username = _server.PacketReader.ReadMessage();        // Username
                var timestamp = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"); // Or use actual timestamp if available

                // Create a new Message object using the data
                var msg = new Message
                {
                    message = messageContent,
                    username = username,
                    timestamp = timestamp
                };

                // Add the Message object to the collection
                Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
            }
        }


        private void UserConnected()
        {
            if (_server.PacketReader != null)
            {
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
        }

        // INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // SetProperty Helper method to avoid redundant code
        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
