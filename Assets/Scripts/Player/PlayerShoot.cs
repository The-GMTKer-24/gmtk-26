using System;
using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerShoot : MonoBehaviour, IShoot
    {
        [SerializeField] private Bullet bullet;
        [SerializeField] private GameObject shootSound;

        private int currentBullets;
        private float reloadTimer;
        private float lastShotTimer;

        private bool held;


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
            

            if (reloadTimer > 0 || lastShotTimer > 0)
            {
                return;
            }
            if (currentBullets == 0)
            {
                Reload();
                return;
            }
            if (!held) return;


            
            currentBullets -= 1;
            lastShotTimer = Player.Instance.PlayerModifier.Evaluate(PlayerStat.TimeBetweenShots);
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldSpace = PlayerManager.Instance.playerCamera.ScreenToWorldPoint(mousePosition);
            worldSpace.z = 0;
            var b = Instantiate(bullet, transform.position, Quaternion.identity);
            b.velocity = (worldSpace - transform.position).normalized * Player.Instance.PlayerModifier.Evaluate(PlayerStat.BulletSpeed);
            b.damage = Player.Instance.PlayerModifier.Evaluate(PlayerStat.Damage);

            SoundManager.Instance.CreateSoundAtPosition(shootSound, transform.position);
        }

        public void OnShoot(InputAction.CallbackContext context)
        {
            if (!enabled) return;
            if (context.started || context.performed)
            {
                held = true;
            }
            else if (context.canceled)
            {
                held = false;
            }

        }

        public void Reload(InputAction.CallbackContext context)
        {
            if (!context.started || !enabled) return;
            Reload();
        }

        private void Reload()
        {
            if (reloadTimer > 0) return;
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

        public float GetGunReadyToFirePercentage()
        {
            return lastShotTimer / Player.Instance.PlayerModifier.Evaluate(PlayerStat.TimeBetweenShots);
        }

        public float GetReloadPercentage()
        {
            return reloadTimer / Player.Instance.PlayerModifier.Evaluate(PlayerStat.ReloadSpeed);
        }
    }
}