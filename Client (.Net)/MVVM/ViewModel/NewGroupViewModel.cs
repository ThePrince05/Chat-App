using Chat_App.MVVM.Model;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using User = Chat_App.MVVM.Model.User;

namespace Client__.Net_.MVVM.ViewModel
{
    public class NewGroupViewModel: INotifyPropertyChanged
    {
        private string _selectedColour;
      
        
        private User _user;

        public User User
        {
            get => _user;
            set
            {
                if (_user != value)
                {
                    _user = value;
                    OnPropertyChanged(nameof(User));
                    OnPropertyChanged(nameof(SelectedColour)); // Ensure UI updates when User changes
                }
            }
        }

        public string SelectedColour
        {
            get => User?.SelectedColor ?? "Gray"; // Provide a default color
            set
            {
                if (User != null && User.SelectedColor != value)
                {
                    User.SelectedColor = value;
                    OnPropertyChanged(nameof(SelectedColour));
                    Debug.WriteLine($"Background Colour: {value}");
                }
            }
        }
        public NewGroupViewModel() 
        {
            User = User ?? new User { SelectedColor = "Gray" }; // Ensure User is never null
        }

        // INotifyProperty
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
