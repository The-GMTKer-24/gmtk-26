using System;
using Entity;
using UnityEngine;
using UnityEngine.UI;

namespace Boss
{
    public class BossBar : MonoBehaviour
    {
        [SerializeField] private Image rend;
        private TimeEntity bossHealth;

        public void Update()
        {
            if (bossHealth)
            {
                var scale = rend.transform.localScale;
                scale.y = bossHealth.GetTime() / bossHealth.GetMaxTime();
                rend.transform.localScale = scale;
            }
            else
            {
                if (Boss.Instance)
                {
                    bossHealth = Boss.Instance.GetComponent<TimeEntity>();
                    rend.gameObject.SetActive(true);
                }
            }
        }
    }
}