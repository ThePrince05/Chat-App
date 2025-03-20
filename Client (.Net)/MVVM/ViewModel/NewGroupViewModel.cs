
using Chat_App.Core.Model;
using Client__.Net_.Core;
using Client__.Net_.MVVM.Model;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private MainViewModel _mainViewModel;
        private readonly SQLiteDBService _sqliteDBService;
        private ICommand _saveGroupCommand;
        private ICommand _togglePanelCommand;


        public User User { get; set; }
        public ICommand TogglePanelCommand => _togglePanelCommand;
        public ICommand SaveGroupCommand => _saveGroupCommand;



        public NewGroupViewModel(MainViewModel mainViewModel) 
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _sqliteDBService = new SQLiteDBService();
            LoadUserData();
            InitializeCommands();

        }
      
        private void InitializeCommands()
        {
            _saveGroupCommand = new RelayCommand(SaveGroup);
            _togglePanelCommand = new RelayCommand(_ => TriggerTogglePanel());
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

        private void SaveGroup(object parameter)
        {
           _mainViewModel.SaveGroup(parameter);

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
