using System;
using Entity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CountDown : MonoBehaviour
    {
        [SerializeField] private TimeEntity player;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Gradient gradient;
        [SerializeField] private float flashRate;
        [SerializeField] private AnimationCurve flash;
        public void Update()
        {
            if (player)
            {
                text.color = gradient.Evaluate(1 - (player.GetTime() / player.GetMaxTime()));
                text.SetText(TimeSpan.FromSeconds(player.GetTime()).ToString("m\\:ss\\.fff"));    
            }
        }
    }
}