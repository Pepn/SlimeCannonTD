using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
{
    [SerializeField] private List<Enemy> enemies;
    public ReadOnlyCollection<Enemy> Enemies { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        enemies = new List<Enemy>();
        Enemies = enemies.AsReadOnly();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(enemies);
    }

    public void AddEnemy(Enemy e)
    {
        enemies.Add(e);
        e.OnDeath += delegate { RemoveEnemy(e);};
    }

    private void RemoveEnemy(Enemy e)
    {
        Debug.Log("Removing Enemy from List.");
        enemies.Remove(e);
    }
}
