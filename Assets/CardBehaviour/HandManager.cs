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

        private void Awake()
        {
            DrawCard();
        }

        public void Discard(GameObject discardedCard)
        {
            _handCards.Remove(discardedCard);
        }

        public async void DrawCard()
        {
            for (int i = 0; i < _maxHandSize; i++)
            {
                if (_handCards.Count >= _maxHandSize) return;
                GameObject g = Instantiate(_cardPrefab, _spawnPoint.position, _spawnPoint.rotation);
                _handCards.Add(g);
                g.transform.parent = _handTransform;
                _cardsHandler.ApplyCardData(g);
                UpdateCardPositions();
                await UniTask.WaitForSeconds(0.25f);
            }
        }

        private void UpdateCardPositions()
        {
            if (_handCards.Count == 0) return;
            float cardSpacing = 1f / _maxHandSize;
            float firstCardPosition = 0.5f - (_handCards.Count - 1) * cardSpacing / 2;
            Spline spline = _splineContainer.Spline;
            for (int i = 0; i < _handCards.Count; i++)
            {
                float p = firstCardPosition + i * cardSpacing;
                Vector3 splinePosition = spline.EvaluatePosition(p);
                Vector3 forward = spline.EvaluateTangent(p);
                Vector3 up = spline.EvaluateUpVector(p);
                Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);
                LMotion.Create(_handCards[i].transform.position, splinePosition, 0.25f)
                    .BindToPosition(_handCards[i].transform);
                Quaternion startRot = _handCards[i].transform.localRotation;
                LMotion.Create(startRot, rotation, 0.25f)
                       .BindToLocalRotation(_handCards[i].transform);
            }
        }
    }
}