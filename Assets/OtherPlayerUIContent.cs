using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OtherPlayerUIContent : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI woodUI;
    [SerializeField] public TextMeshProUGUI foodUI;
    [SerializeField] public List<GameObject> hearts = new List<GameObject>();
}
