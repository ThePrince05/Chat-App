using Chat_App.MVVM.Core;
using Chat_App.MVVM.Model;
using Chat_App.Net;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Chat_App.MVVM.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }
        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }

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
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<string>();
            _server = new Server();

            // Subscribe to server events
            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectEvent += RemoveUser;

            // Define commands
            ConnectToServerCommand = new RelayCommand(
                o =>
                {
                    _server.ConnectToServer(Username);
                    ConnectedUsername = Username; // Update ConnectedUsername
                    Username = string.Empty; // Clear the TextBox
                },
                o => !string.IsNullOrEmpty(Username)
            );

            SendMessageCommand = new RelayCommand(
                o =>
                {
                    _server.SendMessageToServer(Message); // Send the message
                    Message = string.Empty; // Clear the TextBox
                },
                o => !string.IsNullOrEmpty(Message) // Enable only when there's text
            );
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

        // INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
