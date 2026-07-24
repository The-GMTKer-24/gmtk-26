using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;
        [SerializeField] public Camera playerCamera;
        [SerializeField] public SpriteRenderer bigBlackBox;

        public void Awake()
        {
            Instance = this;
        }

        public void LoadSceneAfterDelay(string scene, float delay)
        {
            StartCoroutine(LoadScene(scene,delay));
        }

        private IEnumerator LoadScene(string scene, float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(scene);
        }
    }
}