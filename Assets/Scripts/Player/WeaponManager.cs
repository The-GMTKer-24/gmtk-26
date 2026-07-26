using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class WeaponManager : MonoBehaviour
    {
        public bool gunEnabled = true;
        public bool shotgunEnabled = false;
        public bool bombEnabled = false;

        [SerializeField] private PlayerShoot playerShoot;
        [SerializeField] private Shotgun shotgun;
        [SerializeField] private PlayerBomb bomb;
        [SerializeField] private GameObject changeWeaponSound;

        private IShoot active;
        
        public Weapon Active = Weapon.Gun;

        public static WeaponManager Instance;
        
        public void Awake()
        {
            Instance = this;
            active = playerShoot;
        }

        private void PlayChangeSound()
        {
            SoundManager.Instance.CreateSoundAtPosition(changeWeaponSound, transform.position);
        }

        public void ActivateGun(InputAction.CallbackContext context)
        {
            if (!context.started) return;
            if (!gunEnabled) return;
            Active = Weapon.Gun;
            active = playerShoot;
            playerShoot.enabled = true;
            shotgun.enabled = false;
            bomb.enabled = false;
            PlayChangeSound();
        }
        public void ActivateShotgun(InputAction.CallbackContext context)
        {
            if (!context.started) return;
            if (!shotgunEnabled) return;
            Active = Weapon.Shotgun;
            active = shotgun;
            playerShoot.enabled = false;
            shotgun.enabled = true;
            bomb.enabled = false;
            PlayChangeSound();
        }
        public void ActivateBomb(InputAction.CallbackContext context)
        {
            if (!context.started) return;
            if (!bombEnabled) return;
            Active = Weapon.Bomb;
            active = bomb;
            playerShoot.enabled = false;
            shotgun.enabled = false;
            bomb.enabled = true;
            PlayChangeSound();
        }

        public float GetReloadPercentage()
        {
            return active.GetReloadPercentage();
        }

        public int GetBullets()
        {
            return active.GetBullets();
        }

        public float GetMaxBullets()
        {
            return active.GetMaxBullets();
        }

        public float GetGunReadyToFirePercentage()
        {
            return active.GetGunReadyToFirePercentage();
        }
    }

    public enum Weapon
    {
        Gun,
        Shotgun,
        Bomb,
    }
}
