using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class Attack : CardEffect
{
    public int AttackValue = 5;  

    public override void Apply()
    {
        Debug.Log($"Attack did {AttackValue} damage");
    }
}