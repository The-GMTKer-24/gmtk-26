using System;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        private bool paused = false;
        public void Awake()
        {
            Instance = this;
        }

        public void Pause()
        {
            paused = true;
            Time.timeScale = 0;
        }

        public void UnPause()
        {
            paused = false;
            Time.timeScale = 1;
        }

        public bool Paused => paused;
    }
}