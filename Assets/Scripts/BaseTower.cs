using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class BaseTower : MonoBehaviour
{
    [field: SerializeField] public int Id { get;  set; }
    [SerializeField] protected GameObject towerPrefab;
    [SerializeField] protected List<Weapon> weapons;
    public BaseTower()
    {
        Debug.Log("CREATING TOWER..");
    }


}
