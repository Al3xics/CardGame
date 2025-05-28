using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public TextMeshProUGUI playerTitle;
    public TextMeshProUGUI playerClassText;
    public Button readyButton;
    public GameObject readyText;

    [HideInInspector]
    public Player player;

    public void InitUI()
    {
        if (player == null) return;

        playerTitle.text = player.playerName;
        playerClassText.text = player.behavior.GetType().Name.Replace("Behavior", "");

        readyButton.onClick.RemoveAllListeners();
        readyButton.onClick.AddListener(() => GameManager.Instance.PlayerClickedReady(player));
    }

    public void UpdateUI()
    {
        if (player == null) return;

        bool canAct = player.CanAct(GameManager.Instance.CurrentCycle);
        bool isReady = player.IsReady();

        readyButton.gameObject.SetActive(canAct && !isReady);
        readyText.SetActive(canAct && isReady);
    }
}
