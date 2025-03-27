using Chat_App.Core.Model;
using Client__.Net_.MVVM.Model;
using Client__.Net_.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.UI.Notifications;

namespace Client__.Net_.MVVM.ViewModel
{
    public class NotificationViewModel : INotifyPropertyChanged
    {
        private readonly SupabaseService _supabaseService;
        private readonly SQLiteDBService _sqliteDBService;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly MessageTrackerService _messageTrackerService;
        private bool _isNotificationCheckerActive = false; // Control flag for checking notifications
       

        private NotifyIcon _notifyIcon; // NotifyIcon for tray notifications


        public User User { get; set; }
        public ObservableCollection<string> Notifications { get; set; } = new ObservableCollection<string>();



        public NotificationViewModel(SupabaseSettings supabaseSettings)
        {
            _messageTrackerService = App.MessageTrackerService; // Access shared tracker

            _sqliteDBService = new SQLiteDBService();
            _supabaseService = new SupabaseService(new SupabaseSettings
            {
                SupabaseUrl = supabaseSettings.SupabaseUrl,
                SupabaseApiKey = supabaseSettings.SupabaseApiKey
            });

            LoadUserData();

            // Initialize NotifyIcon
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("Assets/Icons/group.ico"), // Use your custom icon path
                Visible = true,
                Text = "Chat App" // Tooltip text when hovering over the tray icon
            };
        }

        // This method starts the notification checking process
        public void StartNotificationChecker()
        {
            if (_isNotificationCheckerActive) return; // Avoid re-initialization
            _isNotificationCheckerActive = true;

            Task.Run(async () =>
            {
                while (_isNotificationCheckerActive)
                {
                    await CheckForNewMessagesAsync();
                    await Task.Delay(5000); // Check every 5 seconds
                }
            });
        }

        // Stop checking notifications
        public void StopNotificationChecker()
        {
            _isNotificationCheckerActive = false;
        }


        private async Task CheckForNewMessagesAsync()
        {
            try
            {
                Debug.WriteLine("Checking for new messages...");

                int userId = await _supabaseService.GetUserIdByUsernameAsync(User.Username);
                if (userId <= 0)
                {
                    Debug.WriteLine("User ID not found. Exiting check.");
                    return;
                }

                List<int> userGroupIds = await _supabaseService.GetUserGroupIdsAsync(userId);
                if (userGroupIds.Count == 0)
                {
                    Debug.WriteLine("User is not part of any groups. Exiting check.");
                    return;
                }

                foreach (var groupId in userGroupIds)
                {
                    var (latestMessage, groupName) = await _supabaseService.FetchLatestGroupMessageAsync(groupId);
                    if (latestMessage == null) continue;

                    // Get the last known message ID from the shared tracker
                    long lastKnownMessageId = _messageTrackerService.GetLastMessageId(groupId);

                    Debug.WriteLine($"Group {groupId}: Last Known ID = {lastKnownMessageId}, Latest ID = {latestMessage.Id}");

                    // If the tracker hasn't been initialized (i.e., it's zero), update it without notifying.
                    if (lastKnownMessageId == 0)
                    {
                        _messageTrackerService.UpdateLastMessageId(groupId, latestMessage.Id);
                        Debug.WriteLine($"Initializing tracker for Group {groupId} with message ID {latestMessage.Id}");
                        continue; // Skip notifications on initial load
                    }

                    // Check if the new message is newer than the last known message and not from the current user
                    if (latestMessage.Id > lastKnownMessageId && latestMessage.username != User.Username)
                    {
                        await ProcessNewMessage(latestMessage, groupName, groupId);

                        // Update the shared message tracker with the new message ID
                        _messageTrackerService.UpdateLastMessageId(groupId, latestMessage.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CheckForNewMessagesAsync: {ex.Message}");
                Notifications.Add("Error checking messages: " + ex.Message);
            }
        }


        private async Task<(Model.Message, string)> GetLatestMessageForGroupAsync(int groupId)
        {
            var (latestMessage, groupName) = await _supabaseService.FetchLatestGroupMessageAsync(groupId);
            if (latestMessage == null)
            {
                Debug.WriteLine($"No new messages found for Group ID {groupId}.");
                return (null, null);
            }

            return (latestMessage, groupName);
        }

        private async Task ProcessNewMessage(Model.Message latestMessage, string groupName, int groupId)
        {
            if (!string.IsNullOrEmpty(latestMessage.username) && latestMessage.username != User.Username)
            {
                Debug.WriteLine($"New message detected in {groupName}: {latestMessage.message}");
                Notifications.Add($"New message in {groupName}: {latestMessage.message}");
                ShowNewMessageNotification(groupName, latestMessage.message);

                // Update the shared message tracker for this group
                _messageTrackerService.UpdateLastMessageId(groupId, latestMessage.Id);
            }
            else
            {
                Debug.WriteLine($"Skipping message from self or empty sender in {groupName}");
            }
        }

        private void ShowNewMessageNotification(string groupName, string message)
        {
            try
            {
                // Check that group name and message are not null or empty
                if (!string.IsNullOrEmpty(groupName) && !string.IsNullOrEmpty(message))
                {
                    // Set the title and message of the notification
                    _notifyIcon.BalloonTipTitle = $"New message in {groupName}";
                    _notifyIcon.BalloonTipText = message;
                    _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                  

                    // Show the notification (balloon tip)
                    _notifyIcon.ShowBalloonTip(5000);  // Display for 5 seconds

                    PlayCustomSound(); // Play custom sound
                }
                else
                {
                    Debug.WriteLine("Group name or message is empty, notification not shown.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing notification: {ex.Message}");
                Notifications.Add("Error showing notification: " + ex.Message);
            }
        }



        private void PlayCustomSound()
        {
            try
            {
                SoundPlayer player = new SoundPlayer("Assets/Sounds/Noti.wav");
                player.Play();
            }
            catch (Exception ex)
            {
                Notifications.Add("Error playing sound: " + ex.Message);
            }
        }

        private void LoadUserData()
        {
            User = _sqliteDBService.LoadUser();
            OnPropertyChanged(nameof(User));
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
