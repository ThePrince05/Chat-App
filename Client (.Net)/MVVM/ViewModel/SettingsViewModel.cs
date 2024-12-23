using Chat_App.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        [ObservableProperty]
        string pinged = "Not Pinged";

        [RelayCommand]
        void Ping()
        {
            var thread = new Thread(PingServer);
            thread.IsBackground = true;
            thread.Start();

        }

        [RelayCommand]
        void Save()
        {
            try
            {
                if (Pinged == "pinged")
                {
                    WeakReferenceMessenger.Default.Send(new ServerSettingsChangedMessage(ServerUrl, ServerPort, SupabaseApiKey, SupabaseProjectURL, Pinged));
                }
                else
                {
                    MessageBox.Show("Please ping IP address: " + Pinged);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //ServerUrl = "kreft-server.duckdns.org";
            //ServerPort = 7893;
            Console.WriteLine("button clicked... ping");
        }

        void PingServer()
        {
            try
            {
                Pinged = "pinging...";
                var ping = new Ping();

                var result = ping.Send(serverUrl);

                if (result.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    Pinged = "pinged";
                }
                //Console.WriteLine("somthing");
                else
                {
                    Pinged = "not pinged";
                }
            }
            catch (ArgumentNullException e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
        }
    }

    public record ServerSettingsChangedMessage(string ServerUrl, int ServerPort, string SupabaseApiKey, string SupabaseProjectURL, string Pinged);
}
