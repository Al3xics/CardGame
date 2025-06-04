using UnityEngine;
using UnityEngine.Serialization;

public class Attack : CardEffect
{
    public int AttackValue = 5;  

    public override void Apply()
    {
        Debug.Log("attack");
    }
}