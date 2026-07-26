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

        public void Awake()
        {
            mainMenuObjects.SetActive(true);
            creditsMenuObjects.SetActive(false);

            musicManager = MusicManager.Instance;
            musicManager.DuckEQToValue(musicDuckInMenu);
            
            speedrunTimerToggle.isOn = PlayerPrefs.GetInt("SpeedrunTimer") > 0;
        }
        
        public void PlayGame()
        {
            PlayerPrefs.Save();
            
            Debug.Log("The system will play game now!");

            if (easyModeToggle.isOn)
            {
                Instantiate(easyModeUpgrade);
            }
            
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
            
            Debug.Log("The system will shut down now!");
            #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        public void Credits()
        {
            PlayerPrefs.Save();
            
            Debug.Log("The system will credits now!");
            
            mainMenuObjects.SetActive(false);
            creditsMenuObjects.SetActive(true);
        }
        public void MainMenu()
        {
            PlayerPrefs.Save();
            
            Debug.Log("The system will MainMenu now!");
            
            SceneManager.LoadScene(mainMenu);
        }

        public void CloseCredits()
        {
            Debug.Log("The system will close credits now!");
            
            mainMenuObjects.SetActive(true);
            creditsMenuObjects.SetActive(false);
        }

        public void ToggleSpeedrunTimer()
        {
            bool speedrunTimer = speedrunTimerToggle.isOn;
            PlayerPrefs.SetInt("SpeedrunTimer", speedrunTimer ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}