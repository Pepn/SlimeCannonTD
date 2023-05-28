using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Card", menuName = "ScriptableObjects/Cards", order = 1)]
public class CardSO : ScriptableObject
{
    [field: SerializeField] public string CardName { get; private set; }
    [field: SerializeField] public Texture CardImage { get; private set; }
    [field: SerializeField] public TowerSO towerSO { get; private set; }
}


