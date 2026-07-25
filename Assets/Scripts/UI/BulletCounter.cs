using System;
using Entity;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BulletCounter : MonoBehaviour
    {
        [SerializeField] private WeaponManager player;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Gradient gradient;
        [SerializeField] private Gradient reloadGradient;
        [SerializeField] private Image weaponStatus;
        [SerializeField] private Image weaponProgress;
        [SerializeField] private Sprite gun;
        [SerializeField] private Sprite shotgun;
        [SerializeField] private Sprite bomb;
        public void Update()
        {
            if (player)
            {
                switch (player.Active)
                {
                    case Weapon.Gun:
                        weaponStatus.sprite = gun;
                        break;
                    case Weapon.Shotgun:
                        weaponStatus.sprite = shotgun;
                        break;
                    case Weapon.Bomb:
                        weaponStatus.sprite = bomb;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                if (player.GetReloadPercentage() > 0)
                {
                    text.color = reloadGradient.Evaluate(1 - player.GetReloadPercentage());
                    var scale = weaponProgress.transform.localScale;
                    scale.y = player.GetReloadPercentage();
                    weaponProgress.transform.localScale = scale;
                    text.SetText($"Reloading...");
                }
                else
                {
                    text.color = gradient.Evaluate(1 - ((float)player.GetBullets() / player.GetMaxBullets()));
                    text.SetText($"{player.GetBullets()}/{player.GetMaxBullets()}");    
                    var scale = weaponProgress.transform.localScale;
                    scale.y = player.GetGunReadyToFirePercentage();
                    weaponProgress.transform.localScale = scale;
                }
            }
        }
    }
}