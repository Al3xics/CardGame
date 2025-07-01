using System;
using UnityEngine;
using UnityEngine.UI;

public class TargetSelectionUI : MonoBehaviour
{
    private Button _button;
    [SerializeField] private ulong _playerID;
    public static event Action<ulong> OnTargetPicked;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void TargetSelection()
    {
        OnTargetPicked?.Invoke(_playerID);
    }

}
