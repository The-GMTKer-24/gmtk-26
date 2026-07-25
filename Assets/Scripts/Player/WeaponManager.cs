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

        private IShoot active;
        
        public Weapon Active = Weapon.Gun;
        
        public void ActivateGun(InputAction.CallbackContext context)
        {
            if (!context.started) return;
            if (!gunEnabled) return;
            Active = Weapon.Gun;
            active = playerShoot;
            playerShoot.enabled = true;
            shotgun.enabled = false;
            bomb.enabled = false;
        }
        public void ActivateShotgun(InputAction.CallbackContext context)
        {
            if (!context.started) return;
            if (!shotgun) return;
            Active = Weapon.Shotgun;
            active = shotgun;
            playerShoot.enabled = false;
            shotgun.enabled = true;
            bomb.enabled = false;
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
    }

    public enum Weapon
    {
        Gun,
        Shotgun,
        Bomb,
    }
}
