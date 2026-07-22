using System;
using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "Enemy Pool", menuName = "Dungeon/Enemy Pool", order = 0)]
    public class EnemyPool : ScriptableObject
    {
        [System.Serializable]
        public struct SpawnData
        {
            public int cost;
            public GameObject enemy;
        }

        public void Awake()
        {
            
        }

        [SerializeField] private int minCost;
        [SerializeField] private int maxCost;
        [SerializeField] private SpawnData[] spawnCost;

        public int MinCost => minCost;
        public int MaxCost => maxCost;
        public SpawnData[] SpawnCosts => spawnCost;
    }
}