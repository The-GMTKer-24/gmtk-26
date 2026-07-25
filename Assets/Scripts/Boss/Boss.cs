using System;
using System.Collections.Generic;
using Entity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Boss
{
    public class Boss : MonoBehaviour
    {
        [SerializeField] private TimeEntity health;
        [Header("Stats")]
        [SerializeField] private List<AttackInfo> attackStats;
        [SerializeField] private float attackCooldown;
        [Header("Spawn Locations")]
        [SerializeField] private List<Transform> clockBoulderPositions;
        [SerializeField] private Transform bulletWavePosition;
        [SerializeField] private List<Transform> bulletShipPositions;

        [Header("Attack Prefabs")] 
        //[SerializeField] private ClockBoulder clockBoulder;
        [SerializeField] private EnemyBullet bossBullet;
        //[SerializeField] private BulletShips bulletShip;
        [SerializeField] private Bomb bomb;
        
        private AttackInfo currentAttack;
        private float attackTimer;
        private float cooldownTimer;
        private bool attacking;

        public void Update()
        {
            if (!attacking)
            {
                if (cooldownTimer > 0)
                {
                    cooldownTimer -= Time.deltaTime;
                }
                else
                {
                    StartAttack();
                }
            }
            else
            {
                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;
                }
                else
                {
                    attacking = false;
                }
            }
        }

        private void StartAttack()
        {
            attacking = true;
            currentAttack = attackStats[Random.Range(0, attackStats.Count)];
            TriggerAttack();
        }

        private void TriggerAttack()
        {
            
        }
    }
    
    
    [System.Serializable]
    struct AttackInfo
    {
        public Attack attack;
        public float damage;
        public float duration;
    }


    internal enum Attack
    {
        ClockBoulder,
        BulletWave,
        BulletShips,
        Bombs,
    }
}