using System;
using UnityEngine;

namespace Upgrades
{
    public class MaxTimeUpgrade : MonoBehaviour
    {
        [SerializeField] private float amount;

        public void Awake()
        {
            Player.Player.Instance.TimeEntity.SetMaxTime(Player.Player.Instance.TimeEntity.GetTime() + amount);
        }
    }
}