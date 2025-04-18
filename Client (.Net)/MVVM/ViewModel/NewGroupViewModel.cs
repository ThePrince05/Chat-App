
using Chat_App.Core.Model;
using Client__.Net_.Core;
using Client__.Net_.MVVM.Model;
using Client__.Net_.UserControls;
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
    public class NewGroupViewModel : INotifyPropertyChanged
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

        private async Task CreateGroupAsync()
        {
            if (string.IsNullOrEmpty(GroupName))
            {
                MessageBox.Show("Please enter a valid group name.");
                return;
            }

            try
            {
                var groupId = await _supabaseService.InsertGroup(GroupName);
                if (groupId > 0)
                {
                    try
                    {
                        // Ensure group members are added before sending the welcome message
                        await Task.Run(() => _supabaseService.AddGroupMembersAsync(groupId, SelectedUsernames, User.Username));
                    }
                    catch (Exception ex)
                    {
                        // Debug.WriteLine($"Error adding group members: {ex.Message}");
                        MessageBox.Show("Group was created, but adding members failed.");
                        return; // Exit early if adding members fails
                    }

                    // Retry sending the welcome message up to 3 times
                    bool messageSent = false;
                    int retryCount = 0;
                    int maxRetries = 3;

                    while (!messageSent && retryCount < maxRetries)
                    {
                        messageSent = await _supabaseService.SaveMessageAsync(User.Username, "Welcome to the group!", groupId);
                        if (!messageSent)
                        {
                            retryCount++;
                            // Debug.WriteLine($"Retry {retryCount}: Failed to insert welcome message.");
                            await Task.Delay(1000); // Small delay before retrying
                        }
                    }

                    if (!messageSent)
                    {
                        // Debug.WriteLine("Failed to insert welcome message after retries.");
                    }

                    MessageBox.Show("Group created successfully!");

                    // Reset Group Name
                    GroupName = string.Empty;
                    OnPropertyChanged(nameof(GroupName));

                    // Clear Selected Usernames
                    SelectedUsernames.Clear();
                    OnPropertyChanged(nameof(SelectedUsernames));

                    // Reload usernames asynchronously
                    await LoadUsernamesAsync();

                    TriggerTogglePanel();
                    RefreshGroupList();

                }
                else
                {
                    MessageBox.Show("Failed to create group.");
                }
            }
            catch (Exception ex)
            {
                // Debug.WriteLine($"Error in CreateGroupAsync: {ex.Message}");
                MessageBox.Show("An error occurred while creating the group.");
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
