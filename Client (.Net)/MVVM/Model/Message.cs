﻿using Chat_App.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client__.Net_.MVVM.Model
{
    // Model class representing a message record in the "Messages" table
    public class Message
    {
        public long Id { get; set; } // Unique identifier for the message
        public string username { get; set; } // Username of the message sender
        public string message { get; set; } // Text content of the message
        public string timestamp { get; set; } // Timestamp when the message was sent

        // ✅ Convert to South African Time (UTC+2) for display
        public string DisplaySentAt
        {
            get
            {
                if (DateTime.TryParse(timestamp, out DateTime utcTime))
                {
                    TimeZoneInfo southAfricaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");
                    DateTime southAfricaTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, southAfricaTimeZone);
                    return southAfricaTime.ToString("yyyy-MM-dd HH:mm"); // Format without milliseconds
                }
                return timestamp; // Return as-is if parsing fails
            }
        }


        // New property to check if the message is from the current user
        private bool _isCurrentUser;
        public bool IsCurrentUser
        {
            get => _isCurrentUser;
            set
            {
                if (_isCurrentUser != value)
                {
                    _isCurrentUser = value;
                    OnPropertyChanged(nameof(IsCurrentUser));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
