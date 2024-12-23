using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client__.Net_.MVVM.ViewModel
{
    [INotifyPropertyChanged]
    public partial class SettingsService
    {
        [ObservableProperty]
        private string serverUrl;

        [ObservableProperty]
        private int serverPort;

        [ObservableProperty]
        string supabaseApiKey;

        [ObservableProperty]
        string supabaseProjectURL;
    }
}
