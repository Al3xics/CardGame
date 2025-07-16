using LitMotion;
using LitMotion.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.Serialization;


namespace Wendogo
{
    //Handles the visual management and behavior of cards in the player's hand
    public class HandManager : MonoBehaviour
    {
        #region Variables

        [SerializeField] private GameObject _cardPrefab; //Prefab for spawning new cards into the hand
        [SerializeField] public int _maxHandSize = 5; //Maximum number of cards allowed in hand
        [SerializeField] private SplineContainer _splineContainer; //Defines the curve layout for card position

        public Transform _spawnPoint; //World position where new cards appear initially
        public Transform _handTransform; //Parent transform that holds all card objects

        //temp
        [SerializeField] CardsHandler _cardsHandler;  //Handles assigning data and visuals to cards

        public List<GameObject> handCards = new(); //List of all cards currently in hand
        public List<GameObject> passiveZoneCards = new();

        public bool _isReplaced;
        public event Action _onHandsfull;

        #endregion

        private void Awake()
        {
            if (_handTransform == null)
                _handTransform = GameObject.FindWithTag("hand").transform;

        }

        public void Discard(GameObject discardedCard)
        {
            //Remove discarded card from hand list
            handCards.Remove(discardedCard);
        }

        public async void DrawCard(CardDataSO cardData)
        {
            //Stop if hand is full
            if (handCards.Count >= _maxHandSize)
            {
                return;
            }

            //Instantiate a new card at the spawn position
            GameObject g = Instantiate(_cardPrefab, _spawnPoint.position, _spawnPoint.rotation);
            handCards.Add(g);

            //Parent the card under the hand transform to show them in the UI
            g.transform.parent = _handTransform;

            //Assign card data (placeholder)
            _cardsHandler.ApplyCardData(g, cardData);

            //Update layout of cards along spline
            UpdateCardPositions();

            g.transform.localScale = Vector3.one;
            //Delay between each card draw
            await UniTask.WaitForSeconds(0.25f);
        }

        private void UpdateCardPositions()
        {
            //Handle case when hand is empty
            if (handCards.Count == 0) return;

            //Calculate spacing and starting position for the first card
            float cardSpacing = 1f / _maxHandSize;
            float firstCardPosition = 0.5f - (handCards.Count - 1) * cardSpacing / 2;

            Spline spline = _splineContainer.Spline;

            for (int i = 0; i < handCards.Count; i++)
            {
                float p = firstCardPosition + i * cardSpacing;

                //Evaluate position and orientation on the spline
                Vector3 splinePosition = spline.EvaluatePosition(p);
                Vector3 forward = spline.EvaluateTangent(p);
                Vector3 up = spline.EvaluateUpVector(p);

                //Compute rotation for card alignment
                Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

                //Animate card movement to position via tween
                LMotion.Create(handCards[i].transform.position, splinePosition, 0.25f)
                    .BindToPosition(handCards[i].transform);

                //Animate card rotation via tween
                Quaternion startRot = handCards[i].transform.localRotation;
                LMotion.Create(startRot, rotation, 0.25f)
                    .BindToLocalRotation(handCards[i].transform);
            }
        }

        public void ToggleOffMovingCards(List<GameObject> cardsInHand)
        {
            foreach (GameObject card in cardsInHand)
            {
                CardDragHandler handler = card.GetComponent<CardDragHandler>();
                handler.enabled = false;
            }
        }

        public void ToggleOnMovingCards(List<GameObject> cardsInHand)
        {
            foreach (GameObject card in cardsInHand)
            {
                CardDragHandler handler = card.GetComponent<CardDragHandler>();
                handler.enabled = true;
            }
        }

        public void AddCardToPassiveZone(GameObject card)
        {
            if (!passiveZoneCards.Contains(card))
            {
                passiveZoneCards.Add(card);
                Debug.Log($"Card {card.name} added to passive zone.");
            }
        }

        public void RemoveCardFromPassiveZone(GameObject card)
        {
            if (passiveZoneCards.Contains(card))
            {
                passiveZoneCards.Remove(card);

                var targetKey = PlayerUI.Instance.CardSpaces
                .Where(kv => kv.Value == card)
                .Select(kv => kv.Key)
                .ToList();

                foreach (var key in targetKey)
                {
                    PlayerUI.Instance.CardSpaces[key] = null;
                }

                Destroy(card); // Détruire visuellement
                Debug.Log($"Card {card.name} removed from passive zone and destroyed.");
            }
        }

        public List<GameObject> GetPassiveZoneCards() => passiveZoneCards;

        public GameObject GetCardGameObjectInPassiveZone(int cardId)
        {
            foreach (var obj in passiveZoneCards)
            {
                var cardData = obj.GetComponent<CardObjectData>().Card;
                if (cardData != null && cardData.ID == cardId)
                {
                    return obj;
                }
            }

            return null;
        }

        public CardDataSO GetCardDataInPassiveZone(int cardId)
        {
            foreach (var obj in passiveZoneCards)
            {
                var cardData = obj.GetComponent<CardObjectData>().Card;
                if (cardData != null && cardData.ID == cardId)
                {
                    return cardData;
                }
            }

            return null;
        }
    }
}