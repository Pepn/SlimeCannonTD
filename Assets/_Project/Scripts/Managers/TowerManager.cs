using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerManager : Singleton<TowerManager>
{
    [SerializeField, Required, FoldoutGroup("References")] private Transform towerParent;
    /// <summary>
    /// Gets the List of all the towers in this level.
    /// </summary>
    [field: SerializeField, ReadOnly, FoldoutGroup("Debug")]
    public List<BaseTower> Towers { get; private set; } = new List<BaseTower>();

    public Action<BaseTower> OnRemoveTower;

    /// <summary>
    /// Removes the Tower from the TowerManager and the destroy the gameobject.
    /// </summary>
    /// <param name="tower">The tower to destroy.</param>
    public void RemoveTower(BaseTower tower)
    {
        Towers.Remove(tower);
        OnRemoveTower?.Invoke(tower);
        Destroy(tower.gameObject);
    }

    public BasicTower CreateTower(GameObject towerPrefab, Vector3 pos, float weight = 1.0f)
    {
        BasicTower tower = Instantiate(towerPrefab, towerParent).GetComponent<BasicTower>();
        tower.Weight = weight;
        tower.SetTowerSize();
        tower.transform.position = new Vector3(pos.x, pos.y, 0) - new Vector3(0, 0, towerPrefab.GetComponent<BoxCollider>().size.z * 0.5f);
        tower.transform.localRotation = Quaternion.Euler(0, 0, 0);
        tower.PlaySpawnAnimation();
        Towers.Add(tower);
        return tower;
    }

    /// <summary>
    /// Converts a towerId from the Int 2d array which just contains the towerIds to the correct tower.
    /// </summary>
    /// <param name="towerId">The TowerId that is defined in the prefab.</param>
    /// <returns>A Tower with the given towerId.</returns>
    public BaseTower GetTower(int towerId)
    {
        foreach (BaseTower tower in Towers)
        {
            if (tower.Id == towerId)
            {
                return tower;
            }
        }

        return null;
    }
}
