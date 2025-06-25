using Sirenix.Serialization;
using UnityEngine;

public class CardObjectData : MonoBehaviour
{
    public CardDataSO Card;

    [OdinSerialize]
    public GroupAttack Effect;

    public bool isSelected;
}
