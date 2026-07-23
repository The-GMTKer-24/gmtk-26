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
        [SerializeField] private PlayerShoot player;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Gradient gradient;
        [SerializeField] private Gradient reloadGradient;
        public void Update()
        {
            if (player)
            {
                if (player.GetReloadPercentage() > 0)
                {
                    text.color = reloadGradient.Evaluate(1 - player.GetReloadPercentage());
                    text.SetText($"Reloading...");
                }
                else
                {
                    text.color = gradient.Evaluate(1 - ((float)player.GetBullets() / player.GetMaxBullets()));
                    text.SetText($"{player.GetBullets()}/{player.GetMaxBullets()}");    
                }
            }
        }
    }
}