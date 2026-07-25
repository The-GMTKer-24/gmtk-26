using System;
using System.Collections;
using System.Collections.Generic;
using Entity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Boss
{
    public class Boss : MonoBehaviour
    {
        public static Boss Instance;
        
        [SerializeField] private TimeEntity health;
        [Header("Stats")]
        [SerializeField] private List<AttackInfo> attackStats;
        [SerializeField] private float attackCooldown;
        [Header("Bullet Wave")]
        [SerializeField] private float bulletWaveOffsetTime;
        [SerializeField] private float bulletWaveVelocity;
        [SerializeField] private int bulletWaveBullets;
        [Header("Bomb lobbing")]
        [SerializeField] private float bombDelay;
        [SerializeField] private float bombVelocity;
        [SerializeField] private int bombsToThrow;
        [Header("Bullet Ships")]
        [SerializeField] private float bulletShipDelay;
        [SerializeField] private float bulletShipSpeed;
        [SerializeField] private int bulletShips;
        [Header("Spawn Locations")]
        [SerializeField] private List<Transform> clockBoulderPositions;
        [SerializeField] private Transform bulletWavePosition;
        [SerializeField] private List<Transform> leftBulletShipPositions;
        [SerializeField] private List<Transform> rightBulletShipPositions;
        
        [Header("Attack Prefabs")] 
        [SerializeField] private ClockBoulder clockBoulder;
        [SerializeField] private EnemyBullet bossBullet;
        [SerializeField] private BulletShip bulletShip;
        [SerializeField] private Bomb bossBomb;
        
        private AttackInfo currentAttack;
        private float attackTimer;
        private float cooldownTimer;
        private bool attacking;

        public void Awake()
        {
            Instance = this;
        }

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
                    cooldownTimer = attackCooldown;
                    attacking = false;
                }
            }
        }

        private void StartAttack()
        {
            attacking = true;
            
            currentAttack = attackStats[Random.Range(0, attackStats.Count)];
            attackTimer = currentAttack.duration;
            TriggerAttack();
        }

        private void TriggerAttack()
        {
            switch (currentAttack.attack)
            {
                case Attack.ClockBoulder:
                    ClockBoulder(currentAttack);
                    break;
                case Attack.BulletWave:
                    BulletWave(currentAttack);
                    break;
                case Attack.BulletShips:
                    BulletShips(currentAttack);
                    break;
                case Attack.Bombs:
                    Bombs(currentAttack);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Bombs(AttackInfo info)
        {
            IEnumerator Bombs(AttackInfo bombInfo)
            {
                for (int i = 0; i < bombsToThrow; i++)
                {
                    Bomb bomb = Instantiate(bossBomb, bulletWavePosition.position, Quaternion.identity);
                    bomb.damage = bombInfo.damage;
                    Vector2 target = Player.Player.Instance.RigidBody.position;
                    bomb.velocity = bombVelocity *
                                    (target - (Vector2)bulletWavePosition.position).normalized;
                    
                    yield return new WaitForSeconds(bombDelay);
                }
            }
            StartCoroutine(Bombs(info));
        }

        private void BulletShips(AttackInfo info)
        {
            print("Printing bullet ships");
            IEnumerator Ships(AttackInfo shipInfo)
            {
                for (int i = 0; i < bulletShips; i++)
                {
                    Vector2 position;
                    Vector2 velocity;
                    if (i % 2 == 0) {
                        position = leftBulletShipPositions[Random.Range(0, leftBulletShipPositions.Count)].transform.position;
                        velocity = new Vector2(bulletShipSpeed, 0);
                    }
                    else {
                        position = rightBulletShipPositions[Random.Range(0, rightBulletShipPositions.Count)].transform.position;
                        velocity = new Vector2(-bulletShipSpeed, 0);
                    }
                    BulletShip ship = Instantiate(bulletShip, position, Quaternion.identity);
                    ship.damage = shipInfo.damage;
                    ship.velocity = velocity;
                    
                    yield return new WaitForSeconds(bulletShipDelay);
                }
            }
            StartCoroutine(Ships(info));
        }

        private void BulletWave(AttackInfo info)
        {
            IEnumerator Wave(AttackInfo waveInfo)
            {
                for (int i = 0; i < bulletWaveBullets; i++)
                {
                    EnemyBullet bullet = Instantiate(bossBullet, bulletWavePosition.position, Quaternion.identity, transform);
                    bullet.damage = waveInfo.damage;
                    Vector2 target = Player.Player.Instance.RigidBody.position;
                    bullet.velocity = bulletWaveVelocity *
                                      (target - (Vector2)bulletWavePosition.position).normalized;
                    bullet.remainingTime = 10000;
                    bullet.GetComponent<Rigidbody2D>().rotation =
                        -Vector2.SignedAngle(target - (Vector2)transform.position, Vector2.up);
                    
                    yield return new WaitForSeconds(bulletWaveOffsetTime);
                }
            }
            StartCoroutine(Wave(info));
        }

        private void ClockBoulder(AttackInfo info)
        {
            
        }
    }
    
    
    [Serializable]
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