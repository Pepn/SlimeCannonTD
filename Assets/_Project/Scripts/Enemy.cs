using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [field: SerializeField] public float MaxHealth { get; private set; }
    [field: SerializeField] public float Health { get; private set; }
    [field: SerializeField] public float Speed { get; private set; }
    private Collider goalCollider;
    private int damage = 10;
    [SerializeField] private Material deathMaterial;

    private NavMeshAgent navMeshAgent;

    private Renderer renderer;

    public event Action OnDeath;

    // Start is called before the first frame update
    void Start()
    {
        Health = MaxHealth;
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = Speed;
        goalCollider = GameManager.Instance.World.EnemyGoal.GetComponent<BoxCollider>();
        navMeshAgent.SetDestination(goalCollider.transform.position);
        renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Health <= 0)
        {
            Die(1.0f);
        }
        
    }

    //Detect collisions between the GameObjects with Colliders attached
    void OnTriggerEnter(Collider collider)
    {
        //Check for a match with the specified name on any GameObject that collides with your GameObject
        if (collider.gameObject.name == "EnemyGoal")
        {
            //If the GameObject's name matches the one you suggest, output this message in the console
            GameManager.Instance.IncreaseHealth(-damage);
            Die(1.0f);
        }
        
    }

    private void Die(float deathAnimationTime)
    {
        renderer.material = deathMaterial;
        DOTween.To(() => navMeshAgent.speed, x => navMeshAgent.speed = x, 0, deathAnimationTime);
        DOTween.To(() => renderer.material.GetFloat("_DissolveAmount"), x => renderer.material.SetFloat("_DissolveAmount", x), 1, deathAnimationTime);

        OnDeath?.Invoke();
        Destroy(this.gameObject, deathAnimationTime);
    }
}
