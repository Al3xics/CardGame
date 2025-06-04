using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wendogo;
using Wendogo.Menu;

public class GameManager : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SessionManager.Instance.ActiveSession.CurrentPlayer.Properties.TryGetValue(SessionConstants.PlayerNamePropertyKey, out var name);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
