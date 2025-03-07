
using Chat_App.Core.Model;
using Client__.Net_.Core;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using User = Chat_App.Core.Model.User;

namespace Client__.Net_.MVVM.ViewModel
{
    public class NewGroupViewModel: INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;
        private readonly SQLiteDBService _sqliteDBService;
        public User User { get; set; }
        public ICommand TogglePanelCommand { get; }

       
       
        public NewGroupViewModel(MainViewModel mainViewModel) 
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            TogglePanelCommand = new RelayCommand(_ => TriggerTogglePanel());
           
            _sqliteDBService = new SQLiteDBService();
            LoadUserData();


        }

        public void TriggerTogglePanel()
        {
            _mainViewModel.TogglePanel();
        }

       
        private void LoadUserData()
        {
            User = _sqliteDBService.LoadUser();
            OnPropertyChanged(nameof(User));
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
