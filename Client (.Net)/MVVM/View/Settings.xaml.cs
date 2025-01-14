using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client__.Net_.MVVM.View
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void NextToServerSettings_Click(object sender, RoutedEventArgs e)
        {
            // Validate Supabase input
            if (string.IsNullOrEmpty(SupabaseUrlTextBox.Text) || string.IsNullOrEmpty(SupabaseApiKeyBox.Password))
            {
                MessageBox.Show("Please fill in all Supabase fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Enable the second tab and switch to it
            ServerSettingsTab.IsEnabled = true;
            SettingsTabControl.SelectedIndex = 1;
        }

        private void FinishSettings_Click(object sender, RoutedEventArgs e)
        {
            // Validate Server input
            if (string.IsNullOrEmpty(ServerIpAddressTextBox.Text) || string.IsNullOrEmpty(ServerPortNumberTextBox.Text))
            {
                MessageBox.Show("Please fill in all Server fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Save or process the settings
            string supabaseUrl = SupabaseUrlTextBox.Text;
            string supabaseApiKey = SupabaseApiKeyBox.Password;
            string serverIp = ServerIpAddressTextBox.Text;
            string serverPort = ServerPortNumberTextBox.Text;

            MessageBox.Show("Settings saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Close the window
            this.Close();
        }
    }
}

