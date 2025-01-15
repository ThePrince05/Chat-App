using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client__.Net_.MVVM.Model
{
    public class ServerSettings : INotifyPropertyChanged
    {
        private string _serverIp;
        private string _serverPort;
        private bool _isDedicatedServerEnabled;

        public string ServerIp
        {
            get => _serverIp;
            set
            {
                if (_serverIp != value)
                {
                    _serverIp = value;
                    OnPropertyChanged(nameof(ServerIp));
                }
            }
        }

        public string ServerPort
        {
            get => _serverPort;
            set
            {
                if (_serverPort != value)
                {
                    _serverPort = value;
                    OnPropertyChanged(nameof(ServerPort));
                }
            }
        }

        public bool IsDedicatedServerEnabled
        {
            get => _isDedicatedServerEnabled;
            set
            {
                if (_isDedicatedServerEnabled != value)
                {
                    _isDedicatedServerEnabled = value;
                    OnPropertyChanged(nameof(IsDedicatedServerEnabled));
                }
            }
        }



        // Validate Server settings
        public bool ValidateServerSettings()
        {
            if (!IsDedicatedServerEnabled) return true; // No validation required if not enabled

            return !string.IsNullOrEmpty(ServerIp) && !string.IsNullOrEmpty(ServerPort);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
