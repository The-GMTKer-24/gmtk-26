using System;
using Player;
using TMPro;
using UnityEngine;

namespace UI
{
    public class SpeedrunClockOverview : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        public void Update()
        {
            text.SetText(
                $"Last time: {TimeSpan.FromSeconds(PersistentData.Instance.CurrentTime):m\\:ss\\.ff} Best Time: {TimeSpan.FromSeconds(PersistentData.Instance.BestTime):m\\:ss\\.ff}");
        }
    }
}