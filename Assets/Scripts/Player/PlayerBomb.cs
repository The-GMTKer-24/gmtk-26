using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerBomb : MonoBehaviour, IShoot
    {
        [SerializeField] private Bomb bomb;
        private AudioSource audioSource;

        private int currentBullets;
        private float reloadTimer;
        private float lastShotTimer;

        private bool held;

        public void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void Start()
        {
            currentBullets = Player.Instance.PlayerModifier.EvaluateInt(PlayerStat.MaxBombAmmo);
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
            lastShotTimer = Player.Instance.PlayerModifier.Evaluate(PlayerStat.TimeBetweenBombs);
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldSpace = PlayerManager.Instance.playerCamera.ScreenToWorldPoint(mousePosition);
            worldSpace.z = 0;
            var b = Instantiate(bomb, transform.position, Quaternion.identity);
            Vector3 velocity = (worldSpace - transform.position).normalized *
                         Player.Instance.PlayerModifier.Evaluate(PlayerStat.BombSpeed);
            if (BehindTheBack.Instance)
            {
                velocity *= -1;
            }

            b.velocity = velocity;
            b.damage = Player.Instance.PlayerModifier.Evaluate(PlayerStat.BombDamage);
            audioSource.Play();
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
            reloadTimer = Player.Instance.PlayerModifier.Evaluate(PlayerStat.BombReloadSpeed);
            currentBullets = Player.Instance.PlayerModifier.EvaluateInt(PlayerStat.MaxBombAmmo);
        }

        public int GetBullets()
        {
            return currentBullets;
        }

        public int GetMaxBullets()
        {
            return Player.Instance.PlayerModifier.EvaluateInt(PlayerStat.MaxBombAmmo);
        }

        public float GetReloadPercentage()
        {
            return reloadTimer / Player.Instance.PlayerModifier.Evaluate(PlayerStat.BombReloadSpeed);
        }
        
        public float GetGunReadyToFirePercentage()
        {
            return 1;
        }
    }


}
