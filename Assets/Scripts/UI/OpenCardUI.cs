using UnityEngine;
using UnityEngine.UI;

namespace Wendogo
{
    public class OpenCardUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Sprite imageWithText;
        private Sprite _image;
        private Button _button;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonImageClick);
        }

        private void OnButtonImageClick()
        {
            _image = GetComponent<Image>().sprite;
            panel.SetActive(true);
            panel.GetComponentInChildren<Image>().sprite = _image;
            panel.GetComponentInChildren<Button>().onClick.AddListener(OnButtonPanelClick);
        }

        private void OnButtonPanelClick()
        {
            panel.SetActive(false);
        }
    }
}