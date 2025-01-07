using System;
using System.Collections.Generic;
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
    }

}
