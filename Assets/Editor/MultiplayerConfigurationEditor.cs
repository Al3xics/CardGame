using UnityEditor;

namespace Wendogo
{

    [CustomEditor(typeof(MultiplayerConfiguration))]
    public class MultiplayerConfigurationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var config = (MultiplayerConfiguration)target;

            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Select the connection type and then configure its settings.",
                MessageType.Info);

            // Connection Settings
            EditorGUILayout.PropertyField(serializedObject.FindProperty("connectionType"));

            EditorGUILayout.Space(10);

            // Only show Direct settings if type is Direct
            if (config.connectionType == ConnectionType.Direct)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("connectionMode"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("listenIpAddress"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("publishIpAddress"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("port"));

                // HelpBox de warning si Direct sans IP
                if (string.IsNullOrWhiteSpace(config.listenIpAddress))
                {
                    EditorGUILayout.HelpBox(
                        "In Direct mode you must enter an IP (listenIpAddress).",
                        MessageType.Warning);
                }
                // HelpBox de warning si Direct sans IP
                if (string.IsNullOrWhiteSpace(config.publishIpAddress))
                {
                    EditorGUILayout.HelpBox(
                        "In Direct mode you must enter an IP (publishIpAddress).",
                        MessageType.Warning);
                }

                EditorGUILayout.Space(10);
            }

            // Session Settings
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sessionName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxPlayers"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isLocked"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isPrivate"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
