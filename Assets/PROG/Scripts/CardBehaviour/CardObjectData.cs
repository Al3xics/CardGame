using Sirenix.Serialization;
using UnityEngine;

public class CardObjectData : MonoBehaviour
{
    public CardDataSO Card;

    [OdinSerialize]
    public Attack Effect;

    public bool isSelected;
}
