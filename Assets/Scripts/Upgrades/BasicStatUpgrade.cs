using System;
using Player;
using UnityEngine;

namespace Upgrades
{
    public class BasicStatUpgrade : MonoBehaviour
    {
        [SerializeField] float factor = 1.5f;
        [SerializeField] private PlayerStat stat;
        [SerializeField] private ModifierType type;
        public void Start()
        {
            Player.Player.Instance.PlayerModifier.AddModifier(stat, type, factor,
                this);
        }
    }
}