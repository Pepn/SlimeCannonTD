using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TargetInfo", menuName = "ScriptableObjects/TargetInfo", order = 1)]
[InlineEditor]
public class TargetInfoSO : ScriptableObject
{
    /// <summary>
    /// The Tower this TargetInfo upgrades to.
    /// </summary>
    [field: SerializeField, AssetsOnly, Required] public GameObject TowerPrefab { get; private set; }

    /// <summary>
    /// The Material contains the Texture that is used to display the Target.
    /// </summary>
    [field: SerializeField, AssetsOnly, Required] public Material TargetImage { get; private set; }

    /// <summary>
    /// Gets the size of the Target Image in grid space.
    /// </summary>
    [field: SerializeField] public Vector2Int Size {get; private set; }
}
