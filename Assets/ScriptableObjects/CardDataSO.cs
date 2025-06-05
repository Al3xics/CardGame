using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardDataSO : ScriptableObject
{
    [Title("Card Visual")]
    [HorizontalGroup("CardData", 100), VerticalGroup("CardData/Left", PaddingBottom = 5)]
    [PreviewField(100, ObjectFieldAlignment.Left), HideLabel]
    public Texture2D CardVisual;

    [Title("Card Stats")]
    [VerticalGroup("CardData/Stats", 10), LabelWidth(75)]
    public string Name;
    
    [VerticalGroup("CardData/Stats"),LabelWidth(75)]
    public CardTypeSO CardType;

    [VerticalGroup("CardData/Stats"), LabelWidth(120), Range(0, 100), Unit(Units.Percent)]
    public float AppearanceChance;

    [VerticalGroup("CardData/Stats"), LabelWidth(120), Range(0, 10)]
    public int Cost;

    [VerticalGroup("CardData/Stats"), LabelWidth(120), Range(0, 10)]
    public int Value;        
    
    [VerticalGroup("CardData/Stats"), LabelWidth(120)]
    public bool isPassive;    
    
    [VerticalGroup("CardData/Stats"), LabelWidth(120)]
    public bool HasTarget;

    [VerticalGroup("CardData/Left"), LabelWidth(200), MinValue(10100), MaxValue(10199)]
    [ShowIf("Toggle"),HideLabel]
    public int ID;
    
    private string _buttonName = "ID";
    [HideInInspector]
    public bool Toggle;

    [VerticalGroup("CardData/Left")]
    [Button("$_buttonName", ButtonSizes.Small, Stretch = false, ButtonAlignment = 0.5f)]
    private void IDButton()
    {
        this.Toggle = !this.Toggle;
    }
}
