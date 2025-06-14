using LitMotion;
using LitMotion.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Cysharp.Threading.Tasks;


namespace Wendogo
{
    public class HandManager : MonoBehaviour
    {
        [SerializeField] private GameObject _cardPrefab;
        [SerializeField] public int _maxHandSize = 5;
        [SerializeField] private SplineContainer _splineContainer;

        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Transform _handTransform;

        //temp
        [SerializeField] CardsHandler _cardsHandler;

        public List<GameObject> _handCards = new List<GameObject>();

        private void OnEnable()
        {
            PlayerController.OnCardUsed += DrawCard;
        }

        private void OnDisable()
        {
            PlayerController.OnCardUsed -= DrawCard;
        }

        public void Discard(GameObject discardedCard)
        {
            //Remove discarded card from hand list
            _handCards.Remove(discardedCard);
        }

        public async void DrawCard()
        {
            //Implement draw card
            for (int i = 0; i < _maxHandSize; i++)
            {
                //Stop if hand is full
                if (_handCards.Count >= _maxHandSize) return;

                //Instantiate a new card at the spawn position
                GameObject g = Instantiate(_cardPrefab, _spawnPoint.position, _spawnPoint.rotation);
                _handCards.Add(g);

                //Parent the card under the hand transform to show them in the UI
                g.transform.parent = _handTransform;

                //Assign card data (placeholder)
                //_cardsHandler.ApplyCardData(g);

                //Update layout of cards along spline
                UpdateCardPositions();

                //Delay between each card draw
                await UniTask.WaitForSeconds(0.25f);
            }
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
    }
}