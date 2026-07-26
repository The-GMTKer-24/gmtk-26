using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroyOnLoad : MonoBehaviour
{
    private static DontDestroyOnLoad instance;

    [SerializeField] private GameObject easyModeObject;

    private void Awake()
    {
        // 1. Prevent duplicate instances when reloading scenes
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // 2. Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 3. Unsubscribe to prevent dangling memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 4. This method triggers automatically whenever a new scene completes loading
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Instantiate(easyModeObject);
    }
}
