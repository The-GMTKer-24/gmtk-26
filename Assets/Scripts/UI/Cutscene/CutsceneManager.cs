using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Cutscene
{
    public class CutsceneManager : MonoBehaviour
    {
        [SerializeField] private CutsceneFrame[] cutsceneFrames;
        [SerializeField] private float fadeDuration;
        [SerializeField] private string nextSceneToLoad = "Main";
    
        private int currentCutsceneIndex = -1;
        private float fadeTimer;

        private void Start()
        {
            for (int i = 0; i < cutsceneFrames.Length; i++)
            {
                cutsceneFrames[i].cutsceneImage.CrossFadeAlpha(0f, 0f, true);
                cutsceneFrames[i].cutsceneImage.gameObject.SetActive(false);
            }
            fadeTimer = 0.1f;

            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.SetVolume(0);
            }
        }
    
        // Update is called once per frame
        void Update()
        {
            fadeTimer -= Time.deltaTime;
            if (fadeTimer >= 0)
            {
                return;
            }

            currentCutsceneIndex++;
        
            if (currentCutsceneIndex >= cutsceneFrames.Length)
            {
                SkipCutscene();
                return;
            }

            // Crossfade between the two images
            if (currentCutsceneIndex > 0)
            {
                cutsceneFrames[currentCutsceneIndex - 1].cutsceneImage.CrossFadeAlpha(0f, fadeDuration, false);                
            }
            cutsceneFrames[currentCutsceneIndex].cutsceneImage.gameObject.SetActive(true);
            cutsceneFrames[currentCutsceneIndex].cutsceneImage.CrossFadeAlpha(1f, fadeDuration, false);
        
            fadeTimer = cutsceneFrames[currentCutsceneIndex].fadeDelay;
        }

        public void SkipCutscene()
        {
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.SetVolume(1);
            }
            SceneManager.LoadScene(nextSceneToLoad);
        }
    }
}