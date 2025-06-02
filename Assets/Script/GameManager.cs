using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public enum Cycle { Day, Night }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public TextMeshProUGUI cycleText;
    public PlayerUI sharedUI;

    [Header("Background")]
    public Image backgroundImage;
    public Color dayColor = new Color(0.8f, 0.9f, 1f);     // bleu ciel
    public Color nightColor = new Color(0.1f, 0.1f, 0.2f); // bleu foncé

    [Header("Joueurs")]
    public List<Player> players = new List<Player>();

    private int currentPlayerIndex = 0;
    public Cycle CurrentCycle { get; private set; } = Cycle.Day;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        CurrentCycle = Cycle.Day;
        cycleText.text = "The sun is shining...";
        backgroundImage.color = dayColor;

        foreach (var player in players)
        {
            player.ResetReady();
        }

        UpdatePanelUI();
    }

    public void PlayerClickedReady(Player player)
    {
        player.SetReady();
        UpdatePanelUI();
        CheckAllReady();
    }

    public void SwitchPlayer(int direction)
    {
        currentPlayerIndex = (currentPlayerIndex + direction + players.Count) % players.Count;
        UpdatePanelUI();
    }

    public void UpdatePanelUI()
    {
        Player current = players[currentPlayerIndex];
        sharedUI.player = current;
        sharedUI.InitUI();
        sharedUI.UpdateUI();
    }

    void CheckAllReady()
    {
        if (CurrentCycle == Cycle.Day)
        {
            foreach (var player in players)
            {
                if (player.behavior is SurvivorBehavior && !player.IsReady())
                    return;
            }

            CurrentCycle = Cycle.Night;
            cycleText.text = "The moon is rising...";
            backgroundImage.color = nightColor;
            UpdatePanelUI();
            Invoke("StartNewDay", 20f);
        }
        else if (CurrentCycle == Cycle.Night)
        {
            foreach (var player in players)
            {
                if (player.behavior is WendogoBehavior && player.IsReady())
                {
                    StartNewDay();
                    return;
                }
            }
        }
    }

    void StartNewDay()
    {
        CurrentCycle = Cycle.Day;
        cycleText.text = "The sun is shining...";
        backgroundImage.color = dayColor;

        foreach (var player in players)
        {
            player.ResetReady();
        }

        UpdatePanelUI();
    }
}
