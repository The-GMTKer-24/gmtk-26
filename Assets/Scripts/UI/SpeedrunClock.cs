using System;
using Player;
using TMPro;
using UnityEngine;

namespace UI
{
    public class SpeedrunClock : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        public void Update()
        {
            text.SetText(TimeSpan.FromSeconds(PersistentData.Instance.CurrentTime).ToString(@"m\:ss\.ff"));
        }
    }
}