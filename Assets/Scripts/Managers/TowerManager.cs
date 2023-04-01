using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class TowerManager : Singleton<TowerManager>
{
    public List<BaseTower> towers;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddTower(BaseTower tower)
    {
        GameManager.Instance.World.AddObject(tower.transform, GameManager.Instance.World.Floor.transform);
        towers.Add(tower);
    }

    public BasicTower CreateTower(GameObject towerPrefab, Vector3 pos)
    {
        BasicTower tower = Instantiate(towerPrefab).GetComponent<BasicTower>();
        tower.transform.position = pos;

        return tower;
    }
}
