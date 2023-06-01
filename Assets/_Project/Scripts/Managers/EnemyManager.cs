using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
{
    [field: SerializeField] public List<Enemy> Enemies { get; private set; }

    private float _currentMaxHealth = 1.0f;
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

    public float CurrentMaxHealth()
    {
        _currentMaxHealth += 1;
        return _currentMaxHealth;
    }
}
