using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BaseTower class.
/// </summary>
public abstract class BaseTower : MonoBehaviour
{
    private static int idCounter = 0;
    [SerializeField] protected GameObject towerPrefab;
    [SerializeField] protected List<Weapon> weapons;
    [field: SerializeField] public float Weight { get; protected set; } = 1.0f;
    [field: SerializeField] public int Id { get; set; }

    protected virtual void Awake()
    {
        Id = idCounter;
        idCounter++;
        Debug.Log($"Creating tower with id {Id}");
    }
}
