using System;
using System.Collections;
using Player;
using UnityEngine;

namespace Upgrades
{
    public class FatalTempo : MonoBehaviour
    {

        public static FatalTempo Instance { get; set; }

        public void Awake()
        {
            Instance = this;
        }

        public void Tick()
        {
            Modifier stat = Player.Player.Instance.PlayerModifier.AddModifier(PlayerStat.Damage,ModifierType.Multiplicative,2.5f,this);
            StartCoroutine(RemoveThing(stat));
        }

        private IEnumerator RemoveThing(Modifier stat)
        {
            yield return new WaitForSeconds(0.1f);
            Player.Player.Instance.PlayerModifier.RemoveModifier(PlayerStat.Damage,stat);
        }
    }
}
