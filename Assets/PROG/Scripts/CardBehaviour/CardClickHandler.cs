using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using LitMotion;
using LitMotion.Extensions;

namespace Wendogo
{
    //Handles player interactions with individual cards via touch
    public class CardClickHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        private CardObjectData _cardObjectData; //Reference to the CardObjectData on this GameObject

        public static event Action<CardObjectData> OnCardClicked; //Event when any card is clicked

        private Vector3 _originalScale;
        private Quaternion _originalRotation;
        
        private CancellationToken cancellationToken;
        private CancellationTokenSource _cts;

        public PlayerController Owner { get; set; } //Define card ownership

        private void Awake()
        {
            //Get the CardObjectData component attached to this GameObject
            _cardObjectData = GetComponent<CardObjectData>();
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

            transform.SetAsLastSibling();

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            CancellationToken cancellationToken = _cts.Token;

            try
            {

                await UniTask.WaitForSeconds(1, cancellationToken: cancellationToken);

                Vector3 zoomedV3 = new Vector3(5, 5, 5);

                transform.rotation = Quaternion.identity;
                LMotion.Create(transform.localScale, zoomedV3, 0.25f)
                    .BindToLocalScale(transform);
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
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

    }
}
