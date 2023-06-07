using System;

namespace Console {
    public class Message {
        private Object origin;
        private string message;
        private MessageType type;

        public Message(string message, Object origin, MessageType messageType) {
            this.message = message;
            this.origin = origin;
            type = messageType;
        }

        public MessageType getMessageType() {
            return type;
        }

        public enum MessageType {
            Debug,
            Log,
            Warning,
            Error,
            Exception,
        }
    }

}