// <copyright file="UpgradeButtonUI.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The Upgrade Button UI class handles the visual parts of the button.
/// </summary>
public class UpgradeButtonUI : MonoBehaviour
{
    /// <summary>
    /// References to the upgrade button.
    /// </summary>
    [field: SerializeField, FoldoutGroup("References")] public Button UpgradeButton { get; private set; }

    /// <summary>
    /// The info text of the upgrade.
    /// </summary>
    [field: SerializeField, FoldoutGroup("References")] public TextMeshProUGUI UpgradeButtonText { get; private set; }

    /// <summary>
    /// Inits the Button by setting the string & onclick values.
    /// </summary>
    /// <param name="upgrade">Upgrade info.</param>
    public void Init(CannonUpgradeSO upgrade)
    {
        UpgradeButtonText.text = upgrade.Description;
    }
}
