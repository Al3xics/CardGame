using UnityEngine;
using UnityEngine.Serialization;

public class Attack : CardEffect
{
    public override void Apply()
    {
        Debug.Log("attack");
    }
}