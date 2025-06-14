using UnityEditor;
using UnityEngine.UIElements;


namespace Wendogo
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