using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class Shotgun : MonoBehaviour
    {
        [SerializeField] private Bullet bullet;
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
            currentBullets = Player.Instance.PlayerModifier.EvaluateInt(PlayerStat.MaxShotgunAmmo);
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
            lastShotTimer = Player.Instance.PlayerModifier.Evaluate(PlayerStat.TimeBetweenShotgunShots);
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldSpace = PlayerManager.Instance.playerCamera.ScreenToWorldPoint(mousePosition);
            worldSpace.z = 0;

            for (int i = 0; i < Player.Instance.PlayerModifier.EvaluateInt(PlayerStat.ShotgunPellets); i++)
            {
                var b = Instantiate(bullet, transform.position, Quaternion.identity);
                Quaternion angle = Quaternion.Euler(0,0,Random.Range(-Player.Instance.PlayerModifier.Evaluate(PlayerStat.ShotgunSpread)/2,Player.Instance.PlayerModifier.Evaluate(PlayerStat.ShotgunSpread)/2));
                b.velocity = angle * (worldSpace - transform.position).normalized * Player.Instance.PlayerModifier.Evaluate(PlayerStat.ShotgunBulletSpeed);
                b.damage = Player.Instance.PlayerModifier.Evaluate(PlayerStat.Damage) * Player.Instance.PlayerModifier.Evaluate(PlayerStat.ShotgunDamageFactor);
            }
            Player.Instance.RigidBody.AddForce((worldSpace - transform.position).normalized * (Player.Instance.PlayerModifier.Evaluate(PlayerStat.ShotgunRecoil) * -1));
            
            audioSource.Play();
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

        public void Reload(InputAction.CallbackContext context)
        {
            if (!context.started) return;
            Reload();
        }

        private void Reload()
        {
            if (reloadTimer > 0) return;
            reloadTimer = Player.Instance.PlayerModifier.Evaluate(PlayerStat.ShotgunReloadSpeed);
            currentBullets = Player.Instance.PlayerModifier.EvaluateInt(PlayerStat.ShotgunMaxBullets);
        }

        public int GetBullets()
        {
            return currentBullets;
        }
        public int GetMaxBullets()
        {
            return Player.Instance.PlayerModifier.EvaluateInt(PlayerStat.ShotgunMaxBullets);
        }

        public float GetReloadPercentage()
        {
            return reloadTimer / Player.Instance.PlayerModifier.Evaluate(PlayerStat.ShotgunReloadSpeed);
        }
    }
}