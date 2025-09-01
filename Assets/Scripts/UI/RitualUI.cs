using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RitualUI : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI woodUI;
    [SerializeField] public TextMeshProUGUI foodUI;
    [SerializeField] public List<GameObject> ritualParts = new List<GameObject>();
    [SerializeField] public List<GameObject> foodGaugeParts = new List<GameObject>();
    [SerializeField] public List<GameObject> woodGaugeParts = new List<GameObject>();
}
