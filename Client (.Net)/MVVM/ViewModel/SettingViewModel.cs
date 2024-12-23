using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client__.Net_.MVVM.ViewModel
{
    public partial class SettingViewModel : ObservableObject
    {
        [ObservableProperty]
        string serverUrl;

        [ObservableProperty]
        string serverPort;
        
        [ObservableProperty]
        string supabaseApiKey;

        [ObservableProperty]
        string supabaseProjectURL;

        [RelayCommand]
        void Ping()
        {
            ServerUrl = "sdfsd";
            Console.WriteLine("button clicked");
        }
    }
}
