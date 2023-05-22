using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
/// <summary>
/// BaseTower class.
/// </summary>
[System.Serializable]
public abstract class BaseTower : MonoBehaviour
{
    private static int idCounter = 1;
    [SerializeField] protected GameObject towerPrefab;
    [SerializeField] protected List<Weapon> weapons;
    [field: SerializeField] public float Weight { get; set; } = 1.0f;
    [field: SerializeField] public int Id { get; set; }

    [SerializeField, Required] private MMF_Player spawnFeedback;

    protected virtual void Awake()
    {
        Id = idCounter;
        idCounter++;
        Debug.Log($"Creating tower with id {Id}");
    }


    /// <summary>
    /// Plays all the Feel feedbacks for the spawning of a new tower.
    /// </summary>
    public void PlaySpawnAnimation()
    {
        spawnFeedback.PlayFeedbacks();
    }

    /// <summary>
    /// Sets the towersize based on the weight. For now its the volume.
    /// </summary>
    public void SetTowerSize()
    {
        transform.localScale = WeightToScale;
    }

    /// <summary>
    /// Sets the towersize based on the weight. For now its the volume.
    /// </summary>
    protected virtual Vector3 WeightToScale => new Vector3(Weight, Weight, Weight);
}
