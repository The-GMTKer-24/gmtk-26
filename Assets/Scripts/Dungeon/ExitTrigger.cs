using System;
using UI;
using UnityEngine;

namespace Dungeon
{
    public class ExitTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject toSpawn;
        
        private SpriteRenderer bigBlackBox;

        [SerializeField] private float fadeInTime;
        [SerializeField] private AnimationCurve fadeIn;
        private bool started;
        private bool fadeOutStarted;
        private float fadeInTimer;

        private void Start()
        {
            bigBlackBox = Player.Player.Instance.BigBlackBox;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                started = true;
                UIManager.Instance.Pause();
                UIManager.Instance.SetContext(UIContext.Transition);
                bigBlackBox.gameObject.SetActive(true);
            }
        }

        public void Update()
        {
            if (started)
            {
                if (!fadeOutStarted)
                {
                    if (fadeInTimer < fadeInTime)
                    {
                        fadeInTimer += Time.unscaledDeltaTime;
                        var color = bigBlackBox.color;
                        color.a = fadeIn.Evaluate(fadeInTimer/ fadeInTime);
                        bigBlackBox.color = color;
                    }
                    else
                    {
                        fadeOutStarted = true;
                        bigBlackBox.gameObject.SetActive(false);
                        UIManager.Instance.SetContext(null);
                        bigBlackBox.gameObject.SetActive(false);
                        UIManager.Instance.UnPause();
                        Instantiate(toSpawn, transform.position, Quaternion.identity);
                    }
                }
            }
        }
    }
}