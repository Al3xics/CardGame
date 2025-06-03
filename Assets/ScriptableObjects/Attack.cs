using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class Attack : CardEffect
{
    [FormerlySerializedAs("dmg")] [SerializeField] private float ze = 1;

    public override void Apply()
    {
        Debug.Log("qsgjgfkh");
    }
}