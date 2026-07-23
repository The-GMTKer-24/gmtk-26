using System;
using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerShoot : MonoBehaviour
    {
        [SerializeField] private Bullet bullet;


        private int currentBullets;
        private float reloadTimer;
        private float lastShotTimer;

        private bool held;
        public void Awake()
        {

        }

        public void Start()
        {
            currentBullets = Player.Instance.PlayerModifier.EvaluateInt(PlayerStat.MaxBullets);
            reloadTimer = 0;
            lastShotTimer = 0;
        }

        public void Update()
        {
            if (reloadTimer > 0)
            {
                reloadTimer -= Time.deltaTime;
                reloadTimer = Mathf.Max(reloadTimer, 0.0f);
            }
            if (lastShotTimer > 0)
            {
                lastShotTimer -= Time.deltaTime;
                lastShotTimer = Mathf.Max(lastShotTimer, 0.0f);
            }

            if (!held) return;
            if (reloadTimer > 0 || lastShotTimer > 0)
            {
                return;
            }

            if (currentBullets == 0)
            {
                Reload();
                return;
            }
            
            currentBullets -= 1;
            lastShotTimer = Player.Instance.PlayerModifier.Evaluate(PlayerStat.TimeBetweenShots);
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldSpace = PlayerManager.Instance.playerCamera.ScreenToWorldPoint(mousePosition);
            worldSpace.z = 0;
            var b = Instantiate(bullet, transform.position, Quaternion.identity);
            b.speed = (worldSpace - transform.position).normalized * Player.Instance.PlayerModifier.Evaluate(PlayerStat.BulletSpeed);
            b.damage = Player.Instance.PlayerModifier.Evaluate(PlayerStat.Damage);
        }

        public void OnShoot(InputAction.CallbackContext context)
        {
            if (context.started || context.performed)
            {
                held = true;
            }
            else if (context.canceled)
            {
                held = false;
            }
        }

        private void Reload()
        {
            reloadTimer = Player.Instance.PlayerModifier.Evaluate(PlayerStat.ReloadSpeed);
            currentBullets = Player.Instance.PlayerModifier.EvaluateInt(PlayerStat.MaxBullets);
        }

        public int GetBullets()
        {
            return currentBullets;
        }
        public int GetMaxBullets()
        {
            return Player.Instance.PlayerModifier.EvaluateInt(PlayerStat.MaxBullets);
        }

        public float GetReloadPercentage()
        {
            return reloadTimer / Player.Instance.PlayerModifier.Evaluate(PlayerStat.ReloadSpeed);
        }
    }
}