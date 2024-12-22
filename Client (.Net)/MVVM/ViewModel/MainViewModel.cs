using Chat_App.MVVM.Core;
using Chat_App.MVVM.Model;
using Chat_App.Net;
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

        public ObservableCollection<UserModel> Users { get; set; } // List of connected users
        public ObservableCollection<string> Messages { get; set; } // List of messages in the chat
        public RelayCommand ConnectToServerCommand { get; set; } // Command to connect to the server
        public RelayCommand SendMessageCommand { get; set; } // Command to send a message
        public RelayCommand LoadMessagesCommand { get; set; } // Command to load messages from Supabase

        private readonly SupabaseService _supabaseService; // Instance of SupabaseService to interact with Supabase

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username)); // Notify UI of changes
            }
        }

        private string _connectedUsername;
        public string ConnectedUsername
        {
            get => _connectedUsername;
            set
            {
                _connectedUsername = value;
                OnPropertyChanged(nameof(ConnectedUsername)); // Notify UI of changes
            }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message)); // Notify UI of changes
            }
        }

        private Server _server;

        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>(); // Initialize user collection
            Messages = new ObservableCollection<string>(); // Initialize message collection
            _server = new Server(); // Initialize the server instance
            _supabaseService = new SupabaseService(); // Initialize SupabaseService instance

            // Subscribe to server events
            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectEvent += RemoveUser;

            // Define commands for UI interactions
            ConnectToServerCommand = new RelayCommand(
                o =>
                {
                    _server.ConnectToServer(Username); // Connect to the server using the provided username
                    ConnectedUsername = Username; // Update ConnectedUsername
                    Username = string.Empty; // Clear the username TextBox
                },
                o => !string.IsNullOrEmpty(Username) // Enable only if username is not empty
            );

            SendMessageCommand = new RelayCommand(
            async o =>
            {
                if (!string.IsNullOrEmpty(Message))
                {
                    var timestamp = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");

                    // Send message to the server
                    _server.SendMessageToServer(Message);

                    // Save the message to Supabase
                    bool isSaved = await _supabaseService.SaveMessageAsync(ConnectedUsername, Message, timestamp);

                    if (isSaved)
                    {
                        Console.WriteLine("Message saved to database successfully.");
                        // Refresh the list view by reloading messages
                        await LoadMessagesAsync();
                    }
                    else
                    {
                        Console.WriteLine("Failed to save message to the database.");
                    }

                    // Clear the message TextBox
                    Message = string.Empty;
                }
            },
            o => !string.IsNullOrEmpty(Message) // Enable only if message is not empty
);


            LoadMessagesCommand = new RelayCommand(
                async o => await LoadMessagesAsync(), // Asynchronous loading of messages
                o => true // Always enabled
            );

            // Load messages from Supabase on startup
            _ = LoadMessagesAsync();

            // Start the polling timer
            InitializePolling();
        }

        private void InitializePolling()
        {
            _pollingTimer = new System.Timers.Timer(5000); // Poll every 5 seconds
            _pollingTimer.Elapsed += async (sender, e) => await PollMessagesAsync();
            _pollingTimer.AutoReset = true;
            _pollingTimer.Enabled = true;
        }

        private async Task PollMessagesAsync()
        {
            await LoadMessagesAsync(); // Reload messages from the database
        }

        private async Task LoadMessagesAsync()
        {
            try
            {
                Console.WriteLine("Loading messages from Supabase..."); // Debug: Start loading messages

                // Fetch messages from Supabase using the SupabaseService
                var messages = await _supabaseService.GetMessagesAsync();

                // Update the Messages collection on the UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Clear(); // Clear existing messages
                    foreach (var msg in messages)
                    {
                        Messages.Add($"[{msg.timestamp}]: {msg.username}: {msg.message}"); // Add new messages
                    }
                });

                Console.WriteLine("Messages loaded successfully."); // Debug: Successfully loaded messages
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading messages: {ex.Message}"); // Debug: Log any errors
            }
        }

        private void RemoveUser()
        {
            if (_server.PacketReader != null)
            {
                // Read user ID of the disconnected user
                var uid = _server.PacketReader.ReadMessage();
                var user = Users.FirstOrDefault(x => x.UID == uid); // Find the user in the collection
                if (user != null)
                {
                    // Remove the user from the collection on the UI thread
                    Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
                }
            }
        }

        private void MessageReceived()
        {
            if (_server.PacketReader != null)
            {
                // Read the received message
                var msg = _server.PacketReader.ReadMessage();
                // Add the message to the Messages collection on the UI thread
                Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
            }
        }

        private void UserConnected()
        {
            if (_server.PacketReader != null)
            {
                // Create a new user model based on the received data
                var user = new UserModel
                {
                    Username = _server.PacketReader.ReadMessage(), // Read username
                    UID = _server.PacketReader.ReadMessage() // Read user ID
                };

                // Add the user to the collection if not already present
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); // Notify UI of property changes
        }
    }
}
