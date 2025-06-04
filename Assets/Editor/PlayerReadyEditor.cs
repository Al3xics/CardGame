using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Wendogo.Menu;

namespace DefaultNamespace
{
    [CustomEditor(typeof(PlayerReady))]
    public class PlayerReadyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "If this script is used on a client interface, leave the Play Button field empty.",
                MessageType.Info);
        }
    }
}