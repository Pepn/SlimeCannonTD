using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnTime;
    private float spawnTimer;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;
        if(spawnTimer > spawnTime)
        {
            spawnTimer = 0;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab) as GameObject;
        enemy.transform.localScale = new Vector3(1, 1, 1);
        enemy.transform.position = this.transform.position + new Vector3(0,0,0.5f);
        enemy.transform.SetParent(this.transform);
        enemy.GetComponent<Enemy>().Init(EnemyManager.Instance.CurrentMaxHealth());
        GameManager.EnemyManager.AddEnemy(enemy.GetComponent<Enemy>());
    }

}
