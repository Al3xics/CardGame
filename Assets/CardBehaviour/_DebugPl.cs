using UnityEngine;
using UnityEngine.InputSystem;

public class _DebugPl : MonoBehaviour
{
    [SerializeField] private Transform _cardTransform;
    [SerializeField] private CardObjectData _cardObjectData;
    [SerializeField] private InputActionAsset _iaa;
    [SerializeField] private HandManager _hand;
    private InputAction _interact;

    private void Start()
    {
        _interact = _iaa.FindAction("Interact");
        PlayerController.Instance.EnableInput();
    }

    void Update()
    {
        if (_interact.WasReleasedThisFrame())
        {
            _hand.DrawCard();

            //_cardObjectData = FindAnyObjectByType<CardObjectData>();
            //if (!_cardObjectData.isSelected)
            //{
                
            //}
            //else
            //{
            //    TweeningManager.CardDown(_cardObjectData.gameObject.transform);
            //    _cardObjectData.isSelected = false;
            //}
        }
    }
}
