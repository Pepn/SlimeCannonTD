using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] private EnemyGoal enemyGoal;
    [SerializeField] private EnemySpawn enemySpawn;

    [field: SerializeField, Required, ChildGameObjectsOnly] public GameObject Floor { get; private set; }
    public EnemyGoal EnemyGoal
    {
        get => enemyGoal;
        private set { }
    }
    public EnemySpawn EnemySpawn{
        get => enemySpawn;
        private set { }
    }

    // Start is called before the first frame update
    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
    }

    public void AddObject(Transform obj, Transform parent)
    {
        obj.SetParent(parent.transform);
        //UpdateNavMesh();
    }

    private void UpdateNavMesh()
    {
        throw new NotImplementedException();
    }

}
