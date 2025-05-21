using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardDataSO : ScriptableObject
{
    [Title("Card Visual")]
    [HorizontalGroup("CardData", 75)]
    [PreviewField(70, ObjectFieldAlignment.Left)]
    [HideLabel]
    public GameObject CardModel;

    [Title("Card Stats")]
    [VerticalGroup("CardData/Stats", 10)]
    [LabelWidth(75)]
    public string Name;
    
    [VerticalGroup("CardData/Stats")]
    [LabelWidth(75)]
    public CardTypeSO CardType;

    [VerticalGroup("CardData/Stats")]
    [LabelWidth(120)]
    [Range(0, 100), Unit(Units.Percent)]
    public float AppearanceChance;

    [VerticalGroup("CardData/Stats")]
    [LabelWidth(120)]
    [Range(0, 10)]
    public int ActionPointCost;

    [VerticalGroup("CardData/Stats")]
    [LabelWidth(120)]
    [Range(0, 10)]
    public int Value;


}
