using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCanvasSwitcher : MonoBehaviour
{
    public TextMeshProUGUI playerTitle;
    public TextMeshProUGUI cycleText;
    public Button leftButton;
    public Button rightButton;
    public Button readyButton;
    public GameObject readyText;

    private int currentPlayer = 0;
    private string[] playerNames = { "Player 1", "Player 2", "Player 3", "Player 4" };
    private bool[] isPlayerReady = new bool[4];

    private enum Cycle { Day, Night }
    private Cycle currentCycle = Cycle.Day;

    void Start()
    {
        currentCycle = Cycle.Day; // ← s'assurer que c'est bien le jour au début
        cycleText.text = "The sun is shining...";

        for (int i = 0; i < isPlayerReady.Length; i++)
            isPlayerReady[i] = false;

        UpdateUI();

        leftButton.onClick.AddListener(SwitchLeft);
        rightButton.onClick.AddListener(SwitchRight);
        readyButton.onClick.AddListener(MarkReady);
    }


    void SwitchLeft()
    {
        currentPlayer = (currentPlayer - 1 + playerNames.Length) % playerNames.Length;
        UpdateUI();
    }

    void SwitchRight()
    {
        currentPlayer = (currentPlayer + 1) % playerNames.Length;
        UpdateUI();
    }

    void MarkReady()
    {
        if (currentCycle == Cycle.Day)
        {
            isPlayerReady[currentPlayer] = true;
            CheckAllReady();
            UpdateUI();
        }
    }

    void CheckAllReady()
    {
        foreach (bool ready in isPlayerReady)
        {
            if (!ready)
                return;
        }

        // Tous les joueurs sont prêts → passer à la nuit
        currentCycle = Cycle.Night;
        cycleText.text = "The moon is rising...";

        // La nuit commence, on ne peut plus interagir
        UpdateUI();

        // Simuler un retour au jour après quelques secondes
        Invoke("StartNewDay", 20f); // 3 secondes de nuit par exemple
    }

    void StartNewDay()
    {
        currentCycle = Cycle.Day;
        cycleText.text = "The sun is shining...";

        // Reset les statuts "ready"
        for (int i = 0; i < isPlayerReady.Length; i++)
        {
            isPlayerReady[i] = false;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        playerTitle.text = playerNames[currentPlayer];

        bool isReady = isPlayerReady[currentPlayer];

        if (currentCycle == Cycle.Day)
        {
            readyButton.gameObject.SetActive(!isReady);  // Affiche le bouton si pas prêt
            readyText.SetActive(isReady);               // Affiche le texte si prêt
        }
        else // Night
        {
            readyButton.gameObject.SetActive(false);    // On désactive tout
            readyText.SetActive(false);
        }
    }


}
