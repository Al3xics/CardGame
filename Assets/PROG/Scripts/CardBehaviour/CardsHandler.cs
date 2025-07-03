using UnityEngine;
using UnityEngine.UI;

namespace Wendogo
{
    //Responsible for applying card data to instantiated card GameObjects
    public class CardsHandler : MonoBehaviour
    {
        [SerializeField] private GameObject _cardPrefab;           //Prefab used to spawn card visuals & apply data to
        //[SerializeField] private Transform _handTransform;         //Parent transform for displaying cards in the player UI
        [SerializeField] private CardDatabaseSO _cardDatabase;     //Reference to the central card database

        private PlayerController _owner;                           //To set reference to the player owner
        public PlayerController Owner { get; set; }                //Exposed setter for ownership assignment

        private void Start()
        {
            //Get the player owner
            _owner = GetComponent<PlayerController>();
        }

        public void ApplyCardData(GameObject card, CardDataSO cardData)
        {
            try
            {
                //Assign ownership to the card's drag handler
                CardDragHandler dragHandler = card.GetComponent<CardDragHandler>();
                dragHandler.Owner = _owner;

                //Assign visual texture to card UI
                Texture2D cardVisual = cardData.CardVisual;
                RawImage rawImage = card.GetComponentInChildren<RawImage>();
                rawImage.texture = cardVisual;

                //Log card draw for debugging
                Debug.Log($"{cardData.name} drawn");

                //Assign data object to card logic container
                CardObjectData data = card.GetComponent<CardObjectData>();
                data.Card = cardData;
            }
            catch (System.ArgumentNullException e)
            {
                //Log and rethrow if critical components are missing
                Debug.Log(e.Message);
                throw;
            }
        }
    }
}
