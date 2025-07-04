using LitMotion;
using LitMotion.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Cysharp.Threading.Tasks;
using System;


namespace Wendogo
{
    //Handles the visual management and behavior of cards in the player's hand
    public class HandManager : MonoBehaviour
    {
        [SerializeField] private GameObject _cardPrefab; //Prefab for spawning new cards into the hand
        [SerializeField] public int _maxHandSize = 5; //Maximum number of cards allowed in hand
        [SerializeField] private SplineContainer _splineContainer; //Defines the curve layout for card position

        public Transform _spawnPoint; //World position where new cards appear initially
        public Transform _handTransform; //Parent transform that holds all card objects

        //temp
        [SerializeField] CardsHandler _cardsHandler;  //Handles assigning data and visuals to cards

        public List<GameObject> _handCards = new List<GameObject>(); //List of all cards currently in hand

        public bool _isReplaced;

        public event Action _onHandsfull;

        private void Awake()
        {
            if (_handTransform == null)
                _handTransform = GameObject.FindWithTag("hand").transform;

        }


        public void Discard(GameObject discardedCard)
        {
            //Remove discarded card from hand list
            _handCards.Remove(discardedCard);
        }

        public async void DrawCard(CardDataSO cardData)
        {
            //Stop if hand is full
            if (_handCards.Count >= _maxHandSize)
            {
                return;
            }

            //Instantiate a new card at the spawn position
            GameObject g = Instantiate(_cardPrefab, _spawnPoint.position, _spawnPoint.rotation);
            _handCards.Add(g);

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
            if (_handCards.Count == 0) return;

            //Calculate spacing and starting position for the first card
            float cardSpacing = 1f / _maxHandSize;
            float firstCardPosition = 0.5f - (_handCards.Count - 1) * cardSpacing / 2;

            Spline spline = _splineContainer.Spline;

            for (int i = 0; i < _handCards.Count; i++)
            {
                float p = firstCardPosition + i * cardSpacing;

                //Evaluate position and orientation on the spline
                Vector3 splinePosition = spline.EvaluatePosition(p);
                Vector3 forward = spline.EvaluateTangent(p);
                Vector3 up = spline.EvaluateUpVector(p);

                //Compute rotation for card alignment
                Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

                //Animate card movement to position via tween
                LMotion.Create(_handCards[i].transform.position, splinePosition, 0.25f)
                    .BindToPosition(_handCards[i].transform);

                //Animate card rotation via tween
                Quaternion startRot = _handCards[i].transform.localRotation;
                LMotion.Create(startRot, rotation, 0.25f)
                    .BindToLocalRotation(_handCards[i].transform);
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

    }
}