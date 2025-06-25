using Sirenix.OdinInspector;
using UnityEngine;
using Wendogo;

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardDataSO : ScriptableObject
{
    [Title("Card Visual")]
    [HorizontalGroup("CardData", 100), VerticalGroup("CardData/Left", PaddingBottom = 5)]
    [PreviewField(100, ObjectFieldAlignment.Left), HideLabel]
    public Texture2D CardVisual; //Preview the visual in the editor

    [Title("Card Stats")]
    [VerticalGroup("CardData/Stats", 10), LabelWidth(75)]
    public string Name; //Name of the card

    [VerticalGroup("CardData/Stats"), LabelWidth(75)]
    public CardEffect CardEffect; //Reference to the card's effect

    [VerticalGroup("CardData/Stats"), LabelWidth(120), Range(0, 100), Unit(Units.Percent)]
    public float AppearanceChance; //Chance this card appears in the deck or pool //Obsolete?

    [VerticalGroup("CardData/Stats"), LabelWidth(120), Range(0, 10)]
    public int Cost; //Cost of playing the card

    [VerticalGroup("CardData/Stats"), LabelWidth(120), Range(0, 10)]
    public int Value; //Value/strength of the card effect

    [VerticalGroup("CardData/Stats"), LabelWidth(120)]
    public bool isPassive; //Whether the card is passive or not

    [VerticalGroup("CardData/Stats"), LabelWidth(120)]
    public bool HasTarget; //Indicates whether the card needs a target to be played

    [VerticalGroup("CardData/Left"), LabelWidth(200), MinValue(10100), MaxValue(10199)]
    [ShowIf("Toggle"), HideLabel]
    public int ID; //Unique identifier for this card

    private string _buttonName = "ID"; 
    [HideInInspector]
    public bool Toggle; //Controls visibility of the ID field

    [VerticalGroup("CardData/Left")]
    [Button("$_buttonName", ButtonSizes.Small, Stretch = false, ButtonAlignment = 0.5f)]
    private void IDButton()
    {
        // Toggles the visibility of the ID field for this card
        this.Toggle = !this.Toggle;
    }
}
