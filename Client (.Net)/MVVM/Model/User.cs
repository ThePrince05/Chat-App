using Newtonsoft.Json;
using System.ComponentModel;
using System.Windows.Media;

namespace Chat_App.MVVM.Model
{
    public class User
    {
        [JsonProperty("userid")]  // Map JSON field to this property
        public int UserId { get; set; }

        [JsonProperty("username")]  // Map JSON field to this property
        public string Username { get; set; }

        [JsonProperty("userpassword")]  // Map JSON field to this property
        public string UserPassword { get; set; }

        [JsonProperty("selectedcolour")]  // Map JSON field to this property
        public string SelectedColor { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
