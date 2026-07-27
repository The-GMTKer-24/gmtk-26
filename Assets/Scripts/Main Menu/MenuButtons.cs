using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Main_Menu
{
    public class MenuButtons : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenuObjects;
        [SerializeField] private GameObject creditsMenuObjects;
        [SerializeField] private GameObject easyModeUpgrade;
        
        [SerializeField] private string sceneToLoad = "Cutscene";
        [SerializeField] private string mainScene = "Main";
        [SerializeField] private string mainMenu = "Main Menu";
        [SerializeField] private Toggle speedrunTimerToggle;
        [SerializeField] private Toggle easyModeToggle;
        
        [SerializeField] private float musicDuckInMenu = 0.1f;
        private MusicManager musicManager;

        public void Start()
        {
            mainMenuObjects.SetActive(true);
            creditsMenuObjects.SetActive(false);

            musicManager = MusicManager.Instance;
            if (musicManager)
                musicManager.DuckEQToValue(musicDuckInMenu);
            
            speedrunTimerToggle.isOn = PlayerPrefs.GetInt("SpeedrunTimer") > 0;
            easyModeToggle.isOn = PlayerPrefs.GetInt("EasyMode") > 0;
        }
        
        public void PlayGame()
        {
            PlayerPrefs.Save();
            
            if (speedrunTimerToggle.isOn)
            {
                SceneManager.LoadScene(mainScene);
            }
            else
            {
                SceneManager.LoadScene(sceneToLoad);
            }
        }
        
        public void QuitGame()
        {
            PlayerPrefs.Save();
            
            #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        public void Credits()
        {
            PlayerPrefs.Save();
            
            
            mainMenuObjects.SetActive(false);
            creditsMenuObjects.SetActive(true);
        }
        public void MainMenu()
        {
            PlayerPrefs.Save();
            
            
            SceneManager.LoadScene(mainMenu);
        }

        public void CloseCredits()
        {
            
            mainMenuObjects.SetActive(true);
            creditsMenuObjects.SetActive(false);
        }

        public void ToggleSpeedrunTimer()
        {
            bool speedrunTimer = speedrunTimerToggle.isOn;
            PlayerPrefs.SetInt("SpeedrunTimer", speedrunTimer ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        public void ToggleEasyMode()
        {
            bool easyMode = easyModeToggle.isOn;
            print(easyModeToggle.isOn);
            PlayerPrefs.SetInt("EasyMode", easyMode ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}