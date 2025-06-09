using UnityEngine;
using UnityEngine.UI;

namespace Wendogo
{
    public class CardsHandler : MonoBehaviour
    {
        [SerializeField] private GameObject _cardPrefab;
        [SerializeField] private Transform _handTransform;
        [SerializeField] private CardDatabaseSO _cardDatabase;
        private PlayerController _owner;

        private void Start()
        {
            _owner = GetComponent<PlayerController>();
        }

        public void ApplyCardData(GameObject card)
        {
            try
            {
                CardClickHandler clickHandler = card.GetComponent<CardClickHandler>();
                clickHandler.Owner = _owner;
                CardDataSO cardData = _cardDatabase.GetDatabaseCardByID(10100);
                Texture2D cardVisual = cardData.CardVisual;
                RawImage rawImage = card.GetComponentInChildren<RawImage>();
                rawImage.texture = cardVisual;
                Debug.Log($"{cardData.name} drawn");
                CardObjectData data = card.GetComponent<CardObjectData>();
                data.Card = cardData;
            }
            catch (System.ArgumentNullException e)
            {
                Debug.Log(e.Message);
                throw;
            }
        }
    }
}
