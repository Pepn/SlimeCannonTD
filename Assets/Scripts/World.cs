using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Vector3 size;
    [SerializeField] private EnemyGoal enemyGoal;
    [SerializeField] private EnemySpawn enemySpawn;

    public EnemyGoal EnemyGoal
    {
        get => enemyGoal;
        private set { }
    }
    public EnemySpawn EnemySpawn{
        get => enemySpawn;
        private set { }
    }

    public GameObject Floor { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        Floor = gameObject.transform.Find("Floor").gameObject;
    }


    // Update is called once per frame
    void Update()
    {
        //Debug.Log(enemies.Count);
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
