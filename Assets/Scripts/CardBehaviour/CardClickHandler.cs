using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine.UI;

namespace Wendogo
{
    //Handles player interactions with individual cards via touch
    public class CardClickHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        private CardObjectData _cardObjectData; //Reference to the CardObjectData on this GameObject
        private RawImage _cardImage;

        public static event Action<CardObjectData> OnCardClicked; //Event when any card is clicked

        private Vector3 _originalScale;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private RawImage _cardVisual;

        private CancellationToken cancellationToken;
        private CancellationTokenSource _cts;

        public PlayerController Owner { get; set; } //Define card ownership

        private void Awake()
        {
            //Get the CardObjectData component attached to this GameObject
            _cardObjectData = GetComponent<CardObjectData>();
            _cardImage =GetComponentInChildren<RawImage>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Broadcast the click event with this card's data
            //OnCardClicked?.Invoke(_cardObjectData);

            ////Toggle card selection through the PlayerController
            //if (!_cardObjectData.isSelected)
            //{
            //    Owner.SelectCard(_cardObjectData);
            //}
            //else
            //{
            //    Owner.DeselectCard(_cardObjectData);
            //}



        }

        public async void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("Finder put down");
            _originalScale = transform.localScale;
            _originalRotation = transform.rotation;
            _originalPosition = transform.localPosition;
            _cardVisual = _cardImage;

            transform.SetAsLastSibling();

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            CancellationToken cancellationToken = _cts.Token;

            try
            {

                await UniTask.WaitForSeconds(1, cancellationToken: cancellationToken);

                Vector3 zoomedV3 = new Vector3(30, 30, 30);

                transform.rotation = Quaternion.identity;
                
                LMotion.Create(transform.localPosition,Vector3.up,0.1f)
                    .BindToLocalPosition(transform);
                LMotion.Create(transform.localScale, zoomedV3, 0.1f)
                    .BindToLocalScale(transform);
                _cardImage.texture = _cardObjectData.Card.EffectVisual;
                Debug.Log("Finger holded");
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("Animation canceled");
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log("Finder removed");

            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }

            transform.localScale = _originalScale;
            transform.rotation = _originalRotation;
            transform.localPosition = _originalPosition;
            _cardImage = _cardVisual;
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

    }
}
