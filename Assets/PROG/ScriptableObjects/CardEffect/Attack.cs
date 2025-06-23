using UnityEngine;
using UnityEngine.Serialization;
using Wendogo;

[System.Serializable]
public class Attack : CardEffect
{
    public int AttackValue = 5;  

    public override void Apply(ulong owner, ulong target, int value)
    {
    
    }
}