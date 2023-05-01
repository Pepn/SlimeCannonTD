using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerManager : Singleton<TowerManager>
{
    [SerializeField, Required, FoldoutGroup("References")] private Transform towerParent;

    /// <summary>
    /// Gets the List of all the towers in this level.
    /// </summary>
    public List<BaseTower> Towers { get; private set; } = new List<BaseTower>();

    /// <summary>
    /// Removes the Tower from the TowerManager and the destroy the gameobject.
    /// </summary>
    /// <param name="tower">The tower to destroy.</param>
    public void RemoveTower(BaseTower tower)
    {
        Towers.Remove(tower);
        Destroy(tower.gameObject);
    }

    public BasicTower CreateTower(GameObject towerPrefab, Vector3 pos)
    {
        BasicTower tower = Instantiate(towerPrefab, towerParent).GetComponent<BasicTower>();
        tower.transform.position = pos;
        tower.transform.localRotation = Quaternion.Euler(0,0,0);
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
