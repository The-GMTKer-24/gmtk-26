using System;
using Player;
using UnityEngine;

namespace Upgrades
{
    public class LastStand : MonoBehaviour
    {
        private Modifier activeModifier;
        [SerializeField] private float start;
        private bool active = false;
        public void Update()
        {
            if (Player.Player.Instance.TimeEntity.GetTime() < start && !active)
            {
                active = true;
                activeModifier =
                    Player.Player.Instance.PlayerModifier.AddModifier(PlayerStat.Damage, ModifierType.Multiplicative,
                        3.0f, this);
            }
            else if (active && Player.Player.Instance.TimeEntity.GetTime() > start)
            {
                active = false;
                Player.Player.Instance.PlayerModifier.RemoveModifiersFromSource(this);
            }
        }
    }
}