using UnityEngine;
using UnityEngine.Android;

public class NativeMicrophonePermission : MonoBehaviour
{
    private void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Debug.Log("Microphone permission already granted.");
            return;
        }

        ShowInfoThenRequestPermission();
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void ShowInfoThenRequestPermission()
    {
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            ShowExplanationDialog(
                activity,
                "Micro requis",
                "Cette application a besoin du micro pour fonctionner correctement.",
                onOk: () =>
                {
                    // Demander la permission Android
                    var callbacks = new PermissionCallbacks();
                    callbacks.PermissionGranted += perm =>
                    {
                        Debug.Log("Permission microphone accordée.");
                    };
                    callbacks.PermissionDenied += perm =>
                    {
                        Debug.LogWarning("Permission microphone refusée.");

                        // Après refus → rediriger vers paramètres
                        ShowExplanationDialog(
                            activity,
                            "Permission bloquée",
                            "Veuillez activer manuellement le micro dans les paramètres.",
                            () => OpenAppSettings(activity),
                            () => Application.Quit()
                        );
                    };

                    Permission.RequestUserPermission(Permission.Microphone, callbacks);
                },
                onCancel: () => Application.Quit()
            );
        }
    }

    private void OpenAppSettings(AndroidJavaObject activity)
    {
        using (var intent = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS"))
        using (var uri = new AndroidJavaClass("android.net.Uri")
            .CallStatic<AndroidJavaObject>("fromParts", "package", activity.Call<string>("getPackageName"), null))
        {
            intent.Call<AndroidJavaObject>("setData", uri);
            activity.Call("startActivity", intent);
        }
    }

    private void ShowExplanationDialog(AndroidJavaObject activity, string title, string message, System.Action onOk, System.Action onCancel)
    {
        activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            AndroidJavaObject builder = new AndroidJavaObject("android.app.AlertDialog$Builder", activity);
            builder.Call<AndroidJavaObject>("setTitle", title);
            builder.Call<AndroidJavaObject>("setMessage", message);
            builder.Call<AndroidJavaObject>("setCancelable", false);

            builder.Call<AndroidJavaObject>("setPositiveButton", "OK", new DialogClickListener(onOk));
            builder.Call<AndroidJavaObject>("setOnCancelListener", new DialogCancelListener(onCancel));

            AndroidJavaObject dialog = builder.Call<AndroidJavaObject>("create");
            dialog.Call("show");
        }));
    }

    private class DialogClickListener : AndroidJavaProxy
    {
        private readonly System.Action _onClick;

        public DialogClickListener(System.Action onClick) : base("android.content.DialogInterface$OnClickListener")
        {
            _onClick = onClick;
        }

        public void onClick(AndroidJavaObject dialog, int which)
        {
            _onClick?.Invoke();
        }
    }

    private class DialogCancelListener : AndroidJavaProxy
    {
        private readonly System.Action _onCancel;

        public DialogCancelListener(System.Action onCancel) : base("android.content.DialogInterface$OnCancelListener")
        {
            _onCancel = onCancel;
        }

        public void onCancel(AndroidJavaObject dialog)
        {
            _onCancel?.Invoke();
        }
    }
#endif
}