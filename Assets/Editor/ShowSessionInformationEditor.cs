using UnityEditor;
using UnityEngine.UIElements;
using Wendogo.Menu;

namespace DefaultNamespace
{
    [CustomEditor(typeof(ShowSessionInformation))]
    public class ShowSessionInformationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "If this script is used on a client interface, leave the Copy Button field empty.",
                MessageType.Info);
        }
    }
}