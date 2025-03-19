
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
using System.Runtime.CompilerServices;
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
        private SupabaseService _supabaseService;
        private ICommand _createGroupCommand;
        private ICommand _togglePanelCommand;


        public User User { get; set; }
        public ObservableCollection<string> UsernamesList { get; set; } = new ObservableCollection<string>();
        public List<string> SelectedUsernames { get; set; } = new List<string>();

        public ICommand TogglePanelCommand => _togglePanelCommand;
        public ICommand CreateGroupCommand => _createGroupCommand;

        private string _groupName;
        public string GroupName
        {
            get => _groupName;
            set
            {
                _groupName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanCreateGroup)); // Notify button state change
            }
        }

        public bool CanCreateGroup => !string.IsNullOrWhiteSpace(GroupName) && SelectedUsernames.Any();

        public NewGroupViewModel(MainViewModel mainViewModel, SupabaseSettings supabaseSettings) 
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _sqliteDBService = new SQLiteDBService();
          

            // Initialize SupabaseService and subscribe to connection failed event
            _supabaseService = new SupabaseService(new SupabaseSettings
            {
                SupabaseUrl = supabaseSettings.SupabaseUrl,
                SupabaseApiKey = supabaseSettings.SupabaseApiKey
            });
            LoadUserData();
            InitializeCommands();

            // Call the function in the constructor
            Task.Run(async () => await LoadUsernamesAsync());

        }
      
        private void InitializeCommands()
        {
            _createGroupCommand = new AsyncRelayCommand(CreateGroupAsync, () => CanCreateGroup);
            _togglePanelCommand = new RelayCommand(_ => TriggerTogglePanel());
        }

        public void TriggerTogglePanel()
        {
            _mainViewModel.TogglePanel();
        }

        private void RefreshGroupList()
        {
            _mainViewModel.LoadUserGroupsAsync();
        }

       
        private void LoadUserData()
        {
            User = _sqliteDBService.LoadUser();
            OnPropertyChanged(nameof(User));
        }

        //private void SaveGroup(object parameter)
        //{
        //   _mainViewModel.SaveGroup(parameter);
        //}

        private async Task CreateGroupAsync()
        {
            // Ensure that a group name is provided
            if (string.IsNullOrEmpty(GroupName))
            {
                MessageBox.Show("Please enter a valid group name.");
                return;
            }

            // Call InsertGroupAsync to create the group and get the groupId
            var groupId = await _supabaseService.InsertGroup(GroupName);
            if (groupId > 0)
            {
                // If the group was created successfully, add the group members
                await _supabaseService.AddGroupMembersAsync(groupId, SelectedUsernames, User.Username);

                MessageBox.Show("Group created and members added successfully!");
                TriggerTogglePanel();  // Close or hide the panel after successful group creation
                RefreshGroupList();
            }
            else
            {
                MessageBox.Show("Failed to create group.");
            }
        }



        public async Task LoadUsernamesAsync()
        {
            var usernames = await _supabaseService.GetAllUsernamesAsync(User.Username);

            App.Current.Dispatcher.Invoke(() =>
            {
                UsernamesList.Clear();
                foreach (var username in usernames)
                {
                    UsernamesList.Add(username);
                }
            });
        }

      


        // INotifyProperty
        public event PropertyChangedEventHandler PropertyChanged;
        internal virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

    }
}
