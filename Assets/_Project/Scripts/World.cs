using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] private Transform enemyMovemementTarget;
    [SerializeField] private EnemySpawn enemySpawn;

    [field: SerializeField, Required, ChildGameObjectsOnly] public GameObject Floor { get; private set; }
    public Transform EnemyMovemementTarget
    {
        get => enemyMovemementTarget;
    }

    public EnemySpawn EnemySpawn{
        get => enemySpawn;
    }

    // Start is called before the first frame update
    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
    }

    private void UpdateNavMesh()
    {
        throw new NotImplementedException();
    }

}
