using System;
using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerShoot : MonoBehaviour
    {
        [SerializeField] private Bullet bullet;
        [SerializeField] private float speed;
        [SerializeField] private int maxBullets;
        [SerializeField] private float timeBetweenShots;
        [SerializeField] private float reloadTime;
        [SerializeField] private float damage;

        private int currentBullets;
        private float reloadTimer;
        private float lastShotTimer;

        public void Awake()
        {
            currentBullets = maxBullets;
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
        }

        public void OnShoot(InputAction.CallbackContext context)
        {
            if (reloadTimer > 0 || lastShotTimer > 0)
            {
                return;
            }

            if (currentBullets == 0)
            {
                Reload();
            }

            if (!context.performed)
            {
                return;
            }
            currentBullets -= 1;
            lastShotTimer = timeBetweenShots;
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldSpace = PlayerManager.Instance.playerCamera.ScreenToWorldPoint(mousePosition);
            worldSpace.z = 0;
            var b = Instantiate(bullet, transform.position, Quaternion.identity);
            b.speed = (worldSpace - transform.position).normalized * speed;
            b.damage = damage;
        }

        private void Reload()
        {
            reloadTimer = reloadTime;
            currentBullets = maxBullets;
        }

        public int GetBullets()
        {
            return currentBullets;
        }
        public int GetMaxBullets()
        {
            return maxBullets;
        }

        public float GetReloadPercentage()
        {
            return reloadTimer / reloadTime;
        }
    }
}