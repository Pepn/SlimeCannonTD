using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TargetInfo", menuName = "ScriptableObjects/TargetInfo", order = 1)]
[InlineEditor]
public class TargetInfoSO : ScriptableObject
{
    [SerializeField] public GameObject towerPrefab;
    [SerializeField] public Material material;
}
