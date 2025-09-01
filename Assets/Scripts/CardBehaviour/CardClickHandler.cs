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
    /// <summary>
    /// Handles player interactions with individual cards via tap/click and long-press preview.
    /// Designed to avoid conflicts with CardDragHandler.
    /// </summary>
    public class CardClickHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        // Data/visuals
        private CardObjectData _cardObjectData;     // Reference to the CardObjectData on this GameObject
        private RawImage _cardImage;                // Card visual
        private CanvasGroup _canvasGroup;

        public static event Action<CardObjectData> OnCardClicked; // Event when any card is clicked

        // Original state (for preview reset)
        private Vector3 _originalScale;
        private Vector3 _originalLocalPosition;
        private Quaternion _originalRotation;
        private Texture _cardTexture;

        // Long-press handling
        private CancellationTokenSource _cts;

        // Coordination flags
        private bool _isDragging;                   // Set from CardDragHandler
        private bool _isPreviewing;                 // True while long-press preview is active

        [Header("Long-Press Settings")]
        [SerializeField] private float _longPressSeconds = 1f;
        [SerializeField] private Vector3 _previewScale = new Vector3(30, 30, 30);
        [SerializeField] private float _previewTween = 0.1f;

        public PlayerController Owner { get; set; } // Define card ownership

        private void Awake()
        {
            _cardObjectData = GetComponent<CardObjectData>();
            _cardImage = GetComponentInChildren<RawImage>(true);
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// Called by CardDragHandler to mark drag intent/state.
        /// </summary>
        public void SetDragging(bool value)
        {
            _isDragging = value;

            // If a drag begins while preview is pending/active, cancel the preview.
            if (value)
            {
                CancelPreviewToken();
                if (_isPreviewing)
                {
                    ResetPreviewVisuals();
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Ignore click/hold logic if a drag is in progress (or about to)
            if (_isDragging) return;

            // Only allow preview when raycasts are enabled (i.e., NOT dragging)
            if (_canvasGroup != null && !_canvasGroup.blocksRaycasts)
                return;

            CacheOriginalState();
            transform.SetAsLastSibling();

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _ = RunLongPressPreviewAsync(token);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // If user actually dragged, do not fight the drag handler.
            if (eventData.dragging || _isDragging)
            {
                CancelPreviewToken();
                // If a preview was active, quietly reset it
                if (_isPreviewing) ResetPreviewVisuals();
                return;
            }

            // Reset only if we changed something for preview
            CancelPreviewToken();
            if (_isPreviewing)
            {
                ResetPreviewVisuals();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Only fire click if there was no preview and no drag
            if (_isDragging || _isPreviewing) return;

            OnCardClicked?.Invoke(_cardObjectData);
        }

        private async UniTaskVoid RunLongPressPreviewAsync(CancellationToken token)
        {
            try
            {
                await UniTask.WaitForSeconds(_longPressSeconds, cancellationToken: token);

                // Activate preview
                _isPreviewing = true;

                transform.rotation = Quaternion.identity;

                // Small vertical lift for feedback (tweak as needed)
                var rt = transform as RectTransform;

                // Move a little up in local space
                LMotion.Create(rt.localPosition, rt.localPosition + Vector3.up * 250, _previewTween)
                       .BindToLocalPosition(transform);


                LMotion.Create(transform.localScale, _previewScale, _previewTween)
                       .BindToLocalScale(transform);

                if (_cardObjectData != null && _cardImage != null && _cardObjectData.Card != null)
                {
                    _cardImage.texture = _cardObjectData.Card.EffectVisual;
                }
            }
            catch (OperationCanceledException)
            {
                // Swallow: user lifted or started drag
            }
        }

        private void CacheOriginalState()
        {
            _originalScale = transform.localScale;
            _originalRotation = transform.rotation;
            _originalLocalPosition = transform.localPosition;
            if (_cardImage != null)
            {
                _cardTexture = _cardImage.texture;
            }
        }

        private void ResetPreviewVisuals()
        {
            // Restore transform & texture
            transform.localScale = _originalScale;
            transform.rotation = _originalRotation;
            transform.localPosition = _originalLocalPosition;

            if (_cardImage != null)
            {
                _cardImage.texture = _cardTexture;
            }

            _isPreviewing = false;
        }

        private void CancelPreviewToken()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
