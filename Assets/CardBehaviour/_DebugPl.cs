using UnityEngine;
using UnityEngine.InputSystem;
using Wendogo;

public class _DebugPl : MonoBehaviour
{
    [SerializeField] private Transform _cardTransform;
    [SerializeField] private CardObjectData _cardObjectData;
    [SerializeField] private InputActionAsset _iaa;
    [SerializeField] private HandManager _hand;
    [SerializeField] private PlayerController _player;
    private InputAction _interact;

    private void Start()
    {
        _interact = _iaa.FindAction("Interact");
        _player.EnableInput();
    }
}
