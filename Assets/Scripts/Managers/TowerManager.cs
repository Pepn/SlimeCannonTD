using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerManager : Singleton<TowerManager>
{
    /// <summary>
    /// List of all the towers in this level
    /// </summary>
    public List<BaseTower> Towers { get; private set; } = new List<BaseTower>();

    public void AddTower(BaseTower tower)
    {
        GameManager.Instance.World.AddObject(tower.transform, GameManager.Instance.World.Floor.transform);
        Towers.Add(tower);
    }

    public BasicTower CreateTower(GameObject towerPrefab, Vector3 pos)
    {
        BasicTower tower = Instantiate(towerPrefab).GetComponent<BasicTower>();
        tower.transform.position = pos;
        tower.transform.rotation = Quaternion.identity;
        return tower;
    }
}
