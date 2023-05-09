using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using PT;

[ExecuteAlways]
public class UpgradeScreen : MonoBehaviour
{
    [SerializeField, FoldoutGroup("References")] List<CannonUpgradeSO> upgradeSO;
    [SerializeField, FoldoutGroup("References")] Transform upgradesContainer;
    [SerializeField, FoldoutGroup("References")] UpgradeButtonUI upgradeButtonPrefab;

    private void OnEnable()
    {
        upgradesContainer.DestroyChildren();
        GenerateUpgradeButtons();
    }

    private void GenerateUpgradeButtons()
    {
        foreach (CannonUpgradeSO upgrade in upgradeSO)
        {
            UpgradeButtonUI button = Instantiate(upgradeButtonPrefab, upgradesContainer);
            button.Init(upgrade);
        }
    }
}
