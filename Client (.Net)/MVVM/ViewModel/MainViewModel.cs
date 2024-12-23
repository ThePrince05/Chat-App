using Chat_App.Net;
using Client__.Net_.MVVM.Model;
using Client__.Net_.MVVM.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Chat_App.MVVM.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        private System.Timers.Timer _pollingTimer;

        [ObservableProperty]
        object currentView;

        [ObservableProperty]
        string settingsVisibility;

        [ObservableProperty]
        private ObservableCollection<UserModel> users = new();

        [ObservableProperty]
        private ObservableCollection<string> messages = new();

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string connectedUsername;

        [ObservableProperty]
        private string message;

        [ObservableProperty]
        string serverUrl;

        [ObservableProperty]
        int serverPort;

        [ObservableProperty]
        string supabaseApiKey;

        [ObservableProperty]
        string supabaseProjectURL;

        private SupabaseService _supabaseService;
        private readonly Server _server;

        public SettingsViewModel SettingVM { get; } = new();

        public MainViewModel()
        {
            WeakReferenceMessenger.Default.Register<ServerSettingsChangedMessage>(this, (r, m) =>
            {
                ServerUrl = m.ServerUrl;
                ServerPort = m.ServerPort;
                supabaseApiKey = m.SupabaseApiKey;
                supabaseProjectURL = m.SupabaseProjectURL;
            });

            CurrentView = SettingVM;
            Console.WriteLine(CurrentView.ToString());
            _server = new Server();

            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectEvent += RemoveUser;

            //InitializePolling();

            //// Load messages from Supabase on startup
            //_ = LoadMessagesAsync();
        }

        [RelayCommand]
        void Settings()
        {
            if (SettingsVisibility == "Hidden")
                SettingsVisibility = "Visible";
            else
                SettingsVisibility = "Hidden";
        }

        [RelayCommand]
        private void ConnectToServer()
        {
            Console.WriteLine($"the url is {ServerUrl}");
            Console.WriteLine($"the url is {ServerPort}");
            _server.ConnectToServer(Username, ServerUrl, ServerPort);
            ConnectedUsername = Username;
            Username = string.Empty;


            _supabaseService = new SupabaseService(supabaseProjectURL, supabaseApiKey);
            //_supabaseService.StartSupabaseService(supabaseApiKey);

            InitializePolling();

            // Load messages from Supabase on startup
            _ = LoadMessagesAsync();
        }

        [RelayCommand]
        private async Task SendMessageAsync()
        {
            if (!string.IsNullOrEmpty(Message))
            {
                var timestamp = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");

                _server.SendMessageToServer(Message);

                bool isSaved = await _supabaseService.SaveMessageAsync(ConnectedUsername, Message, timestamp);

                if (isSaved)
                {
                    Console.WriteLine("Message saved to database successfully.");
                    await LoadMessagesAsync();
                }
                else
                {
                    Console.WriteLine("Failed to save message to the database.");
                }

                Message = string.Empty;
            }
        }

        [RelayCommand]
        private async Task LoadMessagesAsync()
        {
            try
            {
                Console.WriteLine("Loading messages from Supabase...");

                var messages = await _supabaseService.GetMessagesAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Clear();
                    foreach (var msg in messages)
                    {
                        Messages.Add($"[{msg.timestamp}]: {msg.username}: {msg.message}");
                    }
                });

                Console.WriteLine("Messages loaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading messages: {ex.Message}");
            }
        }

        private void InitializePolling()
        {
            _pollingTimer = new System.Timers.Timer(5000);
            _pollingTimer.Elapsed += async (sender, e) => await LoadMessagesAsync();
            _pollingTimer.AutoReset = true;
            _pollingTimer.Enabled = true;
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
                var msg = _server.PacketReader.ReadMessage();
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
    }
}
