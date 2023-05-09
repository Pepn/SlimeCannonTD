using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "CannonUpgrade", menuName = "ScriptableObjects/CannonUpgrade", order = 1)]
[InlineEditor]
public class CannonUpgradeSO : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; } = "Upgrade Name";
    [field: SerializeField] public string Description { get; private set; } = "Upgrades the X.";
    [field: SerializeField] public UnityEvent OnUpgrade { get; private set; }
    [field: SerializeField] public int MaxUpgradeValue { get; private set; } = 10;
    [field: SerializeField, ReadOnly] public int CurrentUpgradeValue { get; private set; } = 1;
}
