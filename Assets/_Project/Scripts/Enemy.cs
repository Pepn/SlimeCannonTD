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

    private int damage = 10;
    [SerializeField] private Material deathMaterial;

    private NavMeshAgent navMeshAgent;

    private Renderer modelRenderer;

    public event Action OnDeath;
    private bool _isDieing = false;
    // Start is called before the first frame update
    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = Speed;
        navMeshAgent.SetDestination(GameManager.Instance.World.EnemyMovemementTarget.position);
        modelRenderer = GetComponent<Renderer>();
    }

    public void Init(float maxHealth)
    {
        MaxHealth = maxHealth;
        Health = MaxHealth;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Health <= 0)
        {
            Die(1.0f);
        }
    }

    public void ChangeHealth(float damage) => Health += damage;

    //Detect collisions between the GameObjects with Colliders attached
    private void OnTriggerEnter(Collider collider)
    {
        //Check for a match with the specified name on any GameObject that collides with your GameObject
        if (collider.gameObject.name == "MainCannon")
        {
            //If the GameObject's name matches the one you suggest, output this message in the console
            GameManager.Instance.IncreaseHealth(-damage);
            Die(1.0f);
        }
    }

    private void Die(float deathAnimationTime)
    {
        if (_isDieing)
        {
            return;
        }

        _isDieing = true;
        modelRenderer.material = deathMaterial;
        DG.Tweening.Sequence sequence = DOTween.Sequence();
        sequence.Append(DOTween.To(() => navMeshAgent.speed, x => navMeshAgent.speed = x, 0, deathAnimationTime)).
                 AppendCallback(() => Destroy(this.gameObject));
        DOTween.To(() => modelRenderer.material.GetFloat("_DissolveAmount"), x => modelRenderer.material.SetFloat("_DissolveAmount", x), 1, deathAnimationTime);

        OnDeath?.Invoke();
    }
}
