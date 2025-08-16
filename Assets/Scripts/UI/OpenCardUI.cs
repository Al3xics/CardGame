using UnityEngine;
using UnityEngine.UI;

namespace Wendogo
{
    public class OpenCardUI : MonoBehaviour
    {
        [SerializeField] private Sprite imageWithText;
        [SerializeField] private GameObject panel;
        private GameObject _panelCard;
        private GameObject _panelButton;
        private Sprite _image;
        private Button _button;
        
        private void Awake()
        {
            _panelButton = panel.transform.GetChild(0).gameObject;
            _panelCard = panel.transform.GetChild(1).gameObject;
            _button = GetComponent<Button>();
            _image = GetComponent<Image>().sprite;
            _button.onClick.AddListener(OnButtonImageClick);
        }

        private void OnButtonImageClick()
        {
            panel.SetActive(true);
            _panelCard.GetComponent<Image>().sprite = _image;
            _panelButton.GetComponent<Button>().onClick.AddListener(OnButtonPanelClick);
            _panelCard.GetComponent<Button>().onClick.AddListener(OnButtonCardClick);
        }

        private void OnButtonPanelClick()
        {
            panel.GetComponentInChildren<Button>().onClick.RemoveListener(OnButtonPanelClick);
            _panelCard.GetComponent<Button>().onClick.RemoveListener(OnButtonCardClick);
            panel.SetActive(false);
        }
        
        private void OnButtonCardClick()
        {
            var currentImage = _panelCard.GetComponent<Image>().sprite;
            
            if (currentImage == imageWithText)
                _panelCard.GetComponent<Image>().sprite = _image;
            else
                _panelCard.GetComponent<Image>().sprite = imageWithText;
        }
    }
}