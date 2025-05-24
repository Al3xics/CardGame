using UnityEngine;

namespace Wendogo.Menu
{
    public class QuitApp : MonoBehaviour
    {
        public void Quit()
        {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_ANDROID || UNITY_STANDALONE
            Application.Quit();
        #endif
        }
    }
}
 