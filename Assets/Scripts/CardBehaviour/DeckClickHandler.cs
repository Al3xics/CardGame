using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Wendogo
{

    public class DeckClickHandler : MonoBehaviour, IPointerClickHandler
    {
        public int DeckId;

        public static event Action<int> OnDeckClicked;

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"Deck {DeckId} cliqué");
            OnDeckClicked?.Invoke(DeckId);
        }
    }
}
