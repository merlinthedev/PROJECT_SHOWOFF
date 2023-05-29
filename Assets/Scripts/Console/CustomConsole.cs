using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Console {
    public class CustomConsole : EditorWindow {

        private List<Message> messages = new List<Message>();

        // Custom console in this new editor window
        [MenuItem("Window/Custom Console")]
        public static void ShowWindow() {
            GetWindow<CustomConsole>("Custom Console");
        }

        private void OnGUI() {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            if (GUILayout.Button("Clear Console")) {
            }
        }

        // Log message to the console window
        public void Log(string message, Object origin, Message.MessageType type) {
            messages.Add(new Message(message, origin, type));
        }

        // Show the messages in the console window
        private void ShowMessages() {
            foreach (var message in messages) {
                switch (message.getMessageType()) {
                    case Message.MessageType.Debug:
                        break;
                    case Message.MessageType.Log:
                        break;
                    case Message.MessageType.Warning:
                        break;
                    case Message.MessageType.Error:
                        break;
                    case Message.MessageType.Exception:
                        break;
                }
            }
        }

    }
}