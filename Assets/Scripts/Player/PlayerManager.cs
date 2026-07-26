using System;
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
        [SerializeField] public AnimationCurve fadeOutCurve;
        private bool fadingOut = false;
        private float delay;
        private float prog;
        public void Awake()
        {
            Instance = this;
            PersistentData.Instance.CurrentTime = 0;
        }

        public void LoadSceneAfterDelay(string scene, float delay)
        {
            StartCoroutine(LoadScene(scene,delay));
        }

        public void Update()
        {
            PersistentData.Instance.CurrentTime += Time.unscaledDeltaTime;
            if (fadingOut)
            {
                prog += Time.deltaTime;
                var color = bigBlackBox.color;
                color.a = fadeOutCurve.Evaluate(prog / delay);
                bigBlackBox.color = color;
            }
        }

        private IEnumerator LoadScene(string scene, float startDelay)
        {
            bigBlackBox.gameObject.SetActive(true);
            delay = startDelay;
            fadingOut = true;
            yield return new WaitForSeconds(startDelay);
            SceneManager.LoadScene(scene);
        }
    }
}