using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        
        [SerializeField] private string mainMenu = "Main Menu";

        private bool paused = false;
        private UIContext? ctx = null;
        public void Awake()
        {
            Instance = this;
        }

        public void Pause()
        {
            paused = true;
            Time.timeScale = 0;
        }

        public void SetContext(UIContext? ctx)
        {
            this.ctx = ctx;
        }

        public UIContext? GetContext()
        {
            return ctx;
        }

        public void UnPause()
        {
            paused = false;
            Time.timeScale = 1;
        }

        public bool Paused => paused;
        
        public void QuitGame()
        {
            
            PlayerPrefs.Save();
            Debug.Log("The system will shut down now!");
            #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        public void ExitToMenu()
        {
            UnPause();
            SceneManager.LoadScene(mainMenu);
        }
    }
    
}