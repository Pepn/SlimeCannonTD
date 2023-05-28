using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Tower", menuName = "ScriptableObjects/Towers", order = 1)]
[InlineEditor]
public class TowerSO : ScriptableObject
{
    [field: SerializeField] public string TowerName { get; private set; }
    [field: SerializeField] public GameObject TowerObject { get; private set; }
    [field: SerializeField] public float Damage { get; private set; }
    [field: SerializeField] public float AttackSpeed { get; private set; }

    [field: SerializeField] public UnityEvent OnAttack { get; private set; }
    [field: SerializeField] public UnityEvent OnSpawn { get; private set; }
    [field: SerializeField] public UnityEvent OnDestroyEvent { get; private set; }


    public void Start()
    {
        OnSpawn?.Invoke();
    }

    public void OnDestroy()
    {
        OnDestroyEvent?.Invoke();
    }
}