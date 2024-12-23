using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client__.Net_.MVVM.ViewModel
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly SettingsService _settingsService;

        [ObservableProperty]
        string serverUrl;

        [ObservableProperty]
        int serverPort;
        
        [ObservableProperty]
        string supabaseApiKey;

        [ObservableProperty]
        string supabaseProjectURL;

        [RelayCommand]
        void Ping()
        {
            //ServerUrl = "kreft-server.duckdns.org";
            //ServerPort = 7893;
            Console.WriteLine("button clicked");
            WeakReferenceMessenger.Default.Send(new ServerSettingsChangedMessage(ServerUrl, ServerPort, SupabaseApiKey, SupabaseProjectURL));
        }
    }

    public record ServerSettingsChangedMessage(string ServerUrl, int ServerPort, string SupabaseApiKey, string SupabaseProjectURL);
}
