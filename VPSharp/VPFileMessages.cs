using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VPSharp
{
    public class VPFileMessages
    {
        private List<VPFileMessage> _messages;

        internal VPFileMessages()
        {
            _messages = new List<VPFileMessage>();
        }

        public ReadOnlyCollection<VPFileMessage> Messages
        {
            get { return new ReadOnlyCollection<VPFileMessage>(_messages); }
        }

        internal void AddMessage(VPFileMessage message)
        {
            _messages.Add(message);
        }
    }

    public enum MessageType
    {
        ERROR,
        WARNING
    }

    public class VPFileMessage
    {
        internal VPFileMessage(MessageType type, string message)
        {
            Type = type;
            Message = message;
        }

        public string Message { get; internal set; }

        public MessageType Type { get; internal set; }
    }
}
