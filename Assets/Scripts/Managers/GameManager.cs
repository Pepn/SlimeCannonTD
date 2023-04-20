using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://www.jacksondunstan.com/articles/3137

[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(EnemyManager))]
[RequireComponent(typeof(TowerManager))]
public class GameManager : Singleton<GameManager>
{
    public static InputManager InputManager { get; private set; }

    public static EnemyManager EnemyManager { get; private set; }

    public static TowerManager TowerManager { get; private set; }

    public int Score { get; private set; }
    public int Health { get; private set; }

    public World World { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        InitGame();
    }

    public void InitGame()
    {
        Score = 0;
        Health = 100;
    }

    public void IncreaseHealth(int amount) => Health += amount;
    public void IncreaseScore(int amount) => Score += amount;


    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void Awake()
    {
        base.Awake();

        //Find the references
        InputManager = GetComponent<InputManager>();
        TowerManager = GetComponent<TowerManager>();
        EnemyManager = GetComponent<EnemyManager>();

        World = FindObjectOfType<World>();

        //Make this game object persistent
        DontDestroyOnLoad(gameObject);
    }
}
