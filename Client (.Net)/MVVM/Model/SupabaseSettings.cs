using System;
using System.ComponentModel;

namespace Client__.Net_.MVVM.Model
{
    public class SupabaseSettings : INotifyPropertyChanged
    {
        private string _supabaseUrl;
        private string _supabaseApiKey;

        // Property for Supabase URL
        public string SupabaseUrl
        {
            get => _supabaseUrl;
            set
            {
                if (_supabaseUrl != value)
                {
                    _supabaseUrl = value;
                    OnPropertyChanged(nameof(SupabaseUrl)); // Notify when SupabaseUrl changes
                }
            }
        }

        // Property for Supabase API Key
        public string SupabaseApiKey
        {
            get => _supabaseApiKey;
            set
            {
                if (_supabaseApiKey != value)
                {
                    _supabaseApiKey = value;
                    OnPropertyChanged(nameof(SupabaseApiKey)); // Notify when SupabaseApiKey changes
                }
            }
        }

        // Validate Supabase settings (checks if URL and API Key are provided)
        public bool ValidateSupabaseSettings()
        {
            // Check if Supabase URL or API Key is empty
            return !string.IsNullOrEmpty(SupabaseUrl) && !string.IsNullOrEmpty(SupabaseApiKey);
        }

        // Event handler for INotifyPropertyChanged to notify UI about changes in properties
        public event PropertyChangedEventHandler PropertyChanged;

        // Helper method to raise PropertyChanged events
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
