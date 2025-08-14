using UnityEngine;
using Wendogo;

public class DarkTrickHandler : MonoBehaviour
{
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private CardDataSO _dtSabotageCard;
    [SerializeField] private CardDataSO _dtRevealCard;

    public void PlaydtSabotageCard(ulong selectedTarget)
    {
        ServerManager.Instance.TransmitPlayedCardRpc(_dtSabotageCard.ID, selectedTarget, -1, -1);
    }
    public void PlaydtSeeCard(ulong selectedTarget)
    {
        ServerManager.Instance.TransmitPlayedCardRpc(_dtRevealCard.ID, selectedTarget, -1, -1);
    }
}
