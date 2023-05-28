using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
{
    [field: SerializeField] public List<Enemy> Enemies { get; private set; }

    void Start()
    {
        Enemies = new List<Enemy>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void AddEnemy(Enemy e)
    {
        Enemies.Add(e);
        e.OnDeath += delegate { RemoveEnemy(e);};
    }

    private void RemoveEnemy(Enemy e)
    {
        Enemies.Remove(e);
    }
}
