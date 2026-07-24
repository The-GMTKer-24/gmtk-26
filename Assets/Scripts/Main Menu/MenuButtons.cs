using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Main_Menu
{
    public class MenuButtons : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenuObjects;
        [SerializeField] private GameObject creditsMenuObjects;
        
        [SerializeField] private string sceneToLoad = "Cutscene";

        public void Awake()
        {
            mainMenuObjects.SetActive(true);
            creditsMenuObjects.SetActive(false);
        }
        
        public void PlayGame()
        {
            PlayerPrefs.Save();
            
            Debug.Log("The system will play game now!");
            SceneManager.LoadScene(sceneToLoad);
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

        public void CloseCredits()
        {
            Debug.Log("The system will close credits now!");
            
            mainMenuObjects.SetActive(true);
            creditsMenuObjects.SetActive(false);
        }
    }
}