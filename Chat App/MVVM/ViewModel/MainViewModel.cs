using Chat_App.MVVM.Core;
using Chat_App.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat_App.MVVM.ViewModel
{
     class MainViewModel
    {
        public RelayCommand ConnectToServerCommand { get; set; }

        public string Username {  get; set; }

        private Server _server;
        public MainViewModel()
        {
            _server = new Server();
            _server.connectedEvent += userConnected;
            ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer(Username), o => !string.IsNullOrEmpty(Username));
        }

        private void userConnected()
        {
            throw new NotImplementedException();
        }
    }
}
