
using Client__.Net_.MVVM.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Collections.Specialized;
using Client__.Net_.MVVM.View;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using Client__.Net_.Core;
using Chat_App.Core.Model;
using System.Net.NetworkInformation;
using System.Net.Http;
using System.Windows.Controls;
using System.Globalization;
using Client__.Net_.Services;
using System.Media;

namespace Client__.Net_.MVVM.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {

      
        private SupabaseService _supabaseService;
        private readonly SQLiteDBService _sqliteDBService;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly MessageTrackerService _messageTrackerService;
        private string _searchQuery;
        private bool _isConnected = false;

        // Track last fetched message ID for each group
        private Dictionary<int, long> _lastFetchedMessageId = new Dictionary<int, long>();

        public System.Timers.Timer PollingTimer { get; private set; }  // Exposed via a property if needed
       


        // Properties
        public SupabaseSettings SupabaseSettings { get; set; }
        public User User { get; set; }
        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();
        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();
        public NewGroupViewModel NewGroupVM { get; set; }
        public NotificationViewModel NotificationVM { get; set; }

        // Commands
        private ICommand _sendMessageCommand;
        private ICommand _openUserProfileEditCommand;
        private ICommand _openUserProfileAddCommand;
        private ICommand _openSettingsCommand;
        private ICommand _deleteGroupCommand;
        private ICommand _searchGroupsCommand;

        public ICommand SendMessageCommand => _sendMessageCommand;
        public ICommand OpenUserProfileEditCommand => _openUserProfileEditCommand;
        public ICommand OpenUserProfileAddCommand => _openUserProfileAddCommand;
        public ICommand OpenSettingsCommand => _openSettingsCommand;
        public ICommand DeleteGroupCommand => _deleteGroupCommand;
        public ICommand SearchGroupsCommand => _searchGroupsCommand;

        // Events
        public event EventHandler OnUserLoginCompleted;
        public event EventHandler OnSettingsCompleted;
        
        public string Username
        {
            get => User?.Username;
            set
            {
                if (User != null)
                {
                    User.Username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

       
        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));

                Debug.WriteLine($"Message updated: {_message}");

                // Update the SendMessageCommand state
                (_sendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _shadeVisiblity;

        public string ShadeVisiblity
        {
            get { return _shadeVisiblity; }
            set { _shadeVisiblity = value; }
        }


        private Group _selectedGroup;
        public Group SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                _selectedGroup = value;
                OnPropertyChanged(nameof(SelectedGroup));
                Debug.WriteLine(_selectedGroup != null
                    ? $"Selected Group set: {_selectedGroup.GroupName}"
                    : "Selected Group set to null.");
                // Notify the command that its state might have changed
                //ShadeVisiblity = "Hidden";
                (_sendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

     
        private bool _isGroupsLoading;
        public bool IsGroupsLoading
        {
            get => _isGroupsLoading;
            set
            {
                _isGroupsLoading = value;
                OnPropertyChanged(nameof(IsGroupsLoading));
            }
        }

        private bool _isMessagesLoading;
        public bool IsMessagesLoading
        {
            get => _isMessagesLoading;
            set
            {
                _isMessagesLoading = value;
                OnPropertyChanged(nameof(IsMessagesLoading));
            }
        }

        private ObservableCollection<Group> _groups = new ObservableCollection<Group>();
        public ObservableCollection<Group> Groups
        {
            get => _groups;
            set
            {
                _groups = value;
                OnPropertyChanged(nameof(Groups));
            }
        }


        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
            }
        }

        // Constructor
        public MainViewModel()
        {
            _messageTrackerService = App.MessageTrackerService; // Get shared service

            _sqliteDBService = new SQLiteDBService();
            _sqliteDBService.InitializeDatabase();

            // Initialize Commands
            InitializeCommands();

            // Load settings and user data
            LoadSettings();

            // Initialize SupabaseService and subscribe to connection failed event
            _supabaseService = new SupabaseService(new SupabaseSettings
            {
                SupabaseUrl = SupabaseSettings.SupabaseUrl,
                SupabaseApiKey = SupabaseSettings.SupabaseApiKey
            });

            _supabaseService.OnConnectionFailed += HandleConnectionFailure;

            // Explicitly running the async method in a background thread
            Task.Run(() => InitializeDatabaseAsync());

            LoadUserData();

            NewGroupVM = new NewGroupViewModel(this, SupabaseSettings);
            NotificationVM = new NotificationViewModel(SupabaseSettings);

            // Start the connection check (polling every 5 seconds)
            StartConnectionCheck();
            
            // Start Notifications
            NotificationVM.StartNotificationChecker();
        }

        public event Action ToggleNewGroupPanel;
        public void TogglePanel()
        {
            ToggleNewGroupPanel?.Invoke();
        }
        public async Task InitializeDatabaseAsync()
        {
            await _supabaseService.InitializeDatabaseSchemaAsync();
        }


        private void InitializeCommands()
        {
            _sendMessageCommand = new RelayCommand(
            async _ =>
                    {
                        if (SelectedGroup != null)
                        {
                            Debug.WriteLine($"Executing SendMessageAsync for Group ID: {SelectedGroup.Id}");
                            await SendMessageAsync(SelectedGroup.Id);
                        }
                        else
                        {
                            Debug.WriteLine("SendMessageAsync: No group selected.");
                            MessageBox.Show("Please select a group before sending a message.");
                        }
                    },
                    _ => !string.IsNullOrEmpty(Message) && SelectedGroup != null
                );

            _deleteGroupCommand = new AsyncRelayCommand(
              async () =>
              {
                  if (SelectedGroup != null)
                  {
                      Debug.WriteLine($"Executing DeleteGroupAsync for Group ID: {SelectedGroup.Id}");

                      var result = MessageBox.Show($"Are you sure you want to delete the group '{SelectedGroup.GroupName}'?",
                                                   "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                      if (result == MessageBoxResult.Yes)
                      {
                          bool isDeleted = await _supabaseService.DeleteGroupAsync(SelectedGroup.Id);

                          if (isDeleted)
                          {
                              Debug.WriteLine("Group deleted successfully.");
                              MessageBox.Show("Group deleted successfully.");
                              Groups.Remove(SelectedGroup);
                          }
                          else
                          {
                              Debug.WriteLine("Failed to delete group.");
                              MessageBox.Show("Failed to delete group. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                          }
                      }
                  }
                  else
                  {
                      Debug.WriteLine("DeleteGroupAsync: No group selected.");
                      MessageBox.Show("Please select a group before attempting to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                  }
              },
                  () => SelectedGroup != null
              );


            _openSettingsCommand = new RelayCommand(_ => OpenSettings());
            _openUserProfileEditCommand = new RelayCommand(_ => OpenUserProfileEdit());
            _openUserProfileAddCommand = new RelayCommand(_ => OpenUserProfileAdd());
            _searchGroupsCommand = new AsyncRelayCommand(
                async () => await SearchGroupsAsync());

        }


        private async Task SearchGroupsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                // Reset the ListView if the search query is empty
                Debug.WriteLine("Search query is empty, resetting ListView to show all groups.");
                
                Groups.Clear(); // Clear the previous search results

                // You may want to call a method to reload all groups here if needed
                await LoadUserGroupsAsync(); // Reload groups when reconnected

                return; // Exit the method since we don't need to search if the query is empty
            }

            // Show skeleton loader while searching
            IsGroupsLoading = true;
            Debug.WriteLine($"Searching for: {SearchQuery}");

            // Perform the search
            var results = await _supabaseService.SearchGroupsByNameAsync(SearchQuery);

            // Update the Groups collection on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                Groups.Clear(); // Clear the previous search results
                foreach (var group in results)
                {
                    Groups.Add(group);
                }
            });

            Debug.WriteLine($"Groups found: {Groups.Count}");

            // Hide skeleton loader after the search finishes
            IsGroupsLoading = false;
        }




        // these ping google to check internet for I don't have loadusergroups
        public async void StartConnectionCheck()
        {
            while (true) // Loop to check connection status periodically
            {
                if (await IsInternetAvailable())
                {
                    if (!_isConnected)
                    {
                        _isConnected = true; // Mark as connected
                        Debug.WriteLine("Internet is back online. Reloading groups...");
                        await LoadUserGroupsAsync(); // Reload groups when reconnected
                        await NewGroupVM.LoadUsernamesAsync();
                    }
                }
                else
                {
                    _isConnected = false; // Mark as disconnected
                    Debug.WriteLine("No internet connection detected.");
                }

                await Task.Delay(5000); // Check every 5 seconds
            }
        }

        private async Task<bool> IsInternetAvailable()
        {
            try
            {
                // Attempt to ping a reliable online service (can be replaced with your own endpoint)
                var response = await _httpClient.GetAsync("https://www.google.com");
                return response.IsSuccessStatusCode; // Return true if the service is reachable
            }
            catch
            {
                return false; // Return false if an exception occurs (e.g., no internet)
            }
        }
        public async Task LoadUserGroupsAsync()
        {
            IsGroupsLoading = true; // Show skeleton loader

            string currentUsername = User.Username;
            var userGroups = await _supabaseService.GetUserGroupsAsync(currentUsername);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Groups.Clear();
                foreach (var group in userGroups)
                {
                    Groups.Add(group);
                }
            });

            IsGroupsLoading = false; // Hide skeleton loader
        }



        private int? _lastOpenedGroupId = null; // ✅ Keeps track of the last opened group

        public void ScrollToLastMessage(int groupId)
        {
            if (Messages == null || Messages.Count == 0)
            {
                Debug.WriteLine("Messages collection is null or empty. Skipping scroll.");
                return;
            }

            // ✅ Only scroll if the user switches to a new group
            if (_lastOpenedGroupId == groupId)
            {
                Debug.WriteLine($"Already opened Group ID {groupId}. Skipping scroll.");
                return;
            }

            if (Application.Current?.Dispatcher == null)
            {
                Debug.WriteLine("Application or Dispatcher is null. Skipping scroll.");
                return;
            }

            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow == null)
                {
                    Debug.WriteLine("MainWindow is null. Skipping scroll.");
                    return;
                }

                var listView = mainWindow.FindName("lvMessageList") as System.Windows.Controls.ListView;
                if (listView == null)
                {
                    Debug.WriteLine("ListView 'lvMessageList' not found in MainWindow. Skipping scroll.");
                    return;
                }

                if (listView.Items.Count > 0)
                {
                    listView.ScrollIntoView(Messages[^1]); // ✅ Scroll to the last message
                    _lastOpenedGroupId = groupId; // ✅ Mark this group as viewed
                    Debug.WriteLine($"Scrolled to last message for Group ID {groupId}.");
                }
                else
                {
                    Debug.WriteLine("ListView is empty. Skipping scroll.");
                }
            });
        }

        public void ScrollToLastMessage()
        {
            if (Messages == null || Messages.Count == 0)
            {
                Debug.WriteLine("Messages collection is empty. Skipping scroll.");
                return;
            }

            Application.Current?.Dispatcher?.InvokeAsync(() =>
            {
                if (Application.Current.MainWindow?.FindName("lvMessageList") is System.Windows.Controls.ListView listView && listView.Items.Count > 0)
                {
                    listView.ScrollIntoView(Messages[^1]); // Scroll to the last message
                    Debug.WriteLine("Scrolled to the last message. (normal)");
                }
                else
                {
                    Debug.WriteLine("ListView 'lvMessageList' not found or empty. Skipping scroll.");
                }
            });
        }



        internal static void OpenUserProfileEdit()
        {
            UserProfileEdit userProfileEditWindow = new();
            userProfileEditWindow.ShowDialog();
        }
        internal static void OpenUserProfileAdd()
        {
            UserProfile userProfileAddWindow = new UserProfile();
            userProfileAddWindow.ShowDialog();
        }

        private static void OpenSettings()
        {
            var settingsWindow = new Settings();
            settingsWindow.ShowDialog();
        }

     

        private void LoadUserData()
        {
            User = _sqliteDBService.LoadUser();
            OnPropertyChanged(nameof(User));
        }

        private void LoadSettings()
        {
            var settingsService = new SQLiteDBService();
            var supabaseSettings = settingsService.LoadSettings();

            SupabaseSettings = supabaseSettings ?? new SupabaseSettings();

            if (!string.IsNullOrEmpty(SupabaseSettings.SupabaseUrl) &&
                Uri.TryCreate(SupabaseSettings.SupabaseUrl, UriKind.Absolute, out _))
            {
                InitializeSupabaseService();
            }

            // Load user theme
            App.SetPrimaryColorFromUserSelection(_sqliteDBService);
        }

        private void InitializeSupabaseService()
        {
            _supabaseService = new SupabaseService(SupabaseSettings);
            Debug.WriteLine("SupabaseService initialized successfully.");
        }
        private async Task SendMessageAsync(int groupId)
        {
            // Prevent sending empty messages
            if (string.IsNullOrEmpty(Message))
                return;

            Debug.WriteLine($"SendMessageAsync: Sending message to group ID {groupId}");

            // Retry attempt counter
            int retryCount = 0;
            bool isSaved = false;

            // Retry sending the message once if the first attempt fails
            while (retryCount < 2 && !isSaved)
            {
                // Save the message first
                isSaved = await _supabaseService.SaveMessageAsync(Username, Message, groupId);

                if (!isSaved)
                {
                    retryCount++;
                    Debug.WriteLine($"SendMessageAsync: Failed to send message. Attempt {retryCount}.");
                    if (retryCount == 2) // After second attempt, show failure message
                    {
                        MessageBox.Show("Failed to send message after retrying.");
                        return; // Exit if the save operation fails after retrying
                    }
                }
                else
                {
                    Debug.WriteLine($"SendMessageAsync: Message successfully saved on attempt {retryCount + 1}.");
                }
            }

            // Only fetch the latest message if save operation is successful
            Message latestMessage = await _supabaseService.GetLatestMessageAsync(groupId);

            if (latestMessage != null)
            {
                // Update the shared message tracker with the ID of the latest message
                _messageTrackerService.UpdateLastMessageId(groupId, latestMessage.Id);
                Debug.WriteLine($"Updated last message ID for Group {groupId}: {latestMessage.Id}");
            }
            else
            {
                Debug.WriteLine($"SendMessageAsync: Could not retrieve latest message for Group {groupId}");
            }

            // Clear the message input field after sending
            Message = string.Empty;
            Debug.WriteLine("SendMessageAsync: Message input cleared.");

            // Delay before scrolling (optional)
            await Task.Delay(5000);
            ScrollToLastMessage();

            // Play notification sound
            PlayNotificationSound();
        }

        // Plays Notification sound
        private void PlayNotificationSound()
        {
            try
            {
                using (var stream = Application.GetResourceStream(new Uri("Assets/Sounds/WaterDrop.wav", UriKind.Relative))?.Stream)
                {
                    if (stream != null)
                    {
                        var player = new SoundPlayer(stream);
                        player.Play();
                    }
                    else
                    {
                        Debug.WriteLine("Failed to load sound file.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing sound: {ex.Message}");
            }
        }
        public void ResetPollingStateForAll()
        {
            _lastFetchedMessageId.Clear(); // Reset last fetched message Ids for all groups
            Debug.WriteLine("Polling state reset for all groups.");
        }

        public void ResetPollingState(int groupId)
        {
            // Reset the polling state for the specific group by setting its last fetched message Id to 0.
            _lastFetchedMessageId[groupId] = 0; // Reset to 0 (or another value depending on your system)
            Debug.WriteLine($"Polling state reset for Group ID {groupId}.");
        }


        public void StopMessagePolling()
        {
            if (PollingTimer != null)
            {
                PollingTimer.Stop();
                PollingTimer.Dispose();
                PollingTimer = null;
                Debug.WriteLine("✅ Stopped message polling.");
            }
        }

        public void StartMessagePolling()
        {
            if (PollingTimer == null)
            {
                PollingTimer = new System.Timers.Timer(5000);
                PollingTimer.Elapsed += async (sender, e) => await PollMessagesAsync();
                PollingTimer.AutoReset = true;
                PollingTimer.Enabled = true;
                Debug.WriteLine("✅ Started message polling.");
            }
        }

        public async Task PollMessagesAsync()
        {
            if (_supabaseService == null || SelectedGroup == null)
            {
                Debug.WriteLine("Service or group is null, skipping polling.");
                return;
            }

            int groupId = SelectedGroup.Id;
            long lastFetchedId = _lastFetchedMessageId.ContainsKey(groupId) ? _lastFetchedMessageId[groupId] : 0;

            Debug.WriteLine($"Polling for new messages after message ID {lastFetchedId}.");

            // Fetch messages with Id > lastFetchedId
            var newMessages = await _supabaseService.GetMessagesSinceIdAsync(groupId, lastFetchedId);

            if (newMessages.Any())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var msg in newMessages)
                    {
                        // Ensure IsCurrentUser flag is set correctly
                        msg.IsCurrentUser = msg.username == User.Username; // Compare to logged-in user
                        Messages.Add(msg);  // Add after setting IsCurrentUser
                    }

                    // Update the last fetched message ID
                    _lastFetchedMessageId[groupId] = newMessages.Max(m => m.Id);

                    Debug.WriteLine($"Fetched {newMessages.Count} new messages for Group ID {groupId}. Last message ID: {_lastFetchedMessageId[groupId]}");

                });
            }
        }


        public async Task LoadMessagesAsync(int groupId)
        {
            if (_supabaseService == null)
            {
                Debug.WriteLine("SupabaseService is not initialized. Cannot load messages.");
                return;
            }

            try
            {
                Debug.WriteLine($"Fetching messages for Group ID {groupId}...");

                long lastFetchedMessageId = _lastFetchedMessageId.ContainsKey(groupId) ? _lastFetchedMessageId[groupId] : 0;
                var messages = await _supabaseService.GetMessagesSinceIdAsync(groupId, lastFetchedMessageId);

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    Messages.Clear();

                    if (messages != null && messages.Any())
                    {
                        foreach (var msg in messages)
                        {
                            msg.IsCurrentUser = msg.username == User.Username; // Compare to logged-in user
                            Messages.Add(msg);
                        }

                        long latestMessageId = messages.Max(m => m.Id);
                        _lastFetchedMessageId[groupId] = latestMessageId;
                        ScrollToLastMessage(groupId);
                    }

                    Debug.WriteLine($"{Messages.Count} messages loaded.");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Error loading messages: {ex.Message}");
            }
        }

        private void HandleConnectionFailure(string message)
        {
            // Show the settings window with the failure message
            MessageBox.Show(message, "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Optionally, open settings window here
            OpenSettings();
        }

        // Function to create ContextMenu dynamically
        public ContextMenu CreateContextMenu()
        {
            var contextMenu = new ContextMenu();
            var deleteMenuItem = new MenuItem
            {
                Header = "Delete Group",
                Command = DeleteGroupCommand,
                CommandParameter = SelectedGroup
            };

            // Make sure the context menu uses the correct DataContext
            contextMenu.DataContext = this;  // Set DataContext to MainViewModel or another relevant context if necessary

            contextMenu.Items.Add(deleteMenuItem);
            return contextMenu;
        }



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
