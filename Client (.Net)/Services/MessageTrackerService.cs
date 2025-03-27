using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client__.Net_.Services
{
    public class MessageTrackerService
    {
        private readonly Dictionary<int, long> _lastMessageIds = new Dictionary<int, long>();

        public void UpdateLastMessageId(int groupId, long messageId)
        {
            if (_lastMessageIds.ContainsKey(groupId))
            {
                _lastMessageIds[groupId] = messageId;
            }
            else
            {
                _lastMessageIds.Add(groupId, messageId);
            }
        }

        public long GetLastMessageId(int groupId)
        {
            return _lastMessageIds.TryGetValue(groupId, out var lastMessageId) ? lastMessageId : 0;
        }
    }

}
