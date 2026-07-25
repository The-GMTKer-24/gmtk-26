using System;
using Player;
using UnityEngine;

namespace Boss
{
    public class BossCameraTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject newTarget;
        [SerializeField] private float size;
        [SerializeField] private float timeToSize;
        [SerializeField] private AnimationCurve sizeCurve;
        private float progress;
        private float startSize;
        private bool started;

        public void Update()
        {
            if (started)
            {
                PlayerManager.Instance.playerCamera.orthographicSize =
                    Mathf.Lerp(startSize, size, sizeCurve.Evaluate(progress / timeToSize));
                progress += Time.deltaTime;
                if (progress > timeToSize)
                {
                    Destroy(gameObject);
                }
            }
            
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerManager.Instance.playerCamera.GetComponent<CameraFollow>().target = newTarget;
                startSize = PlayerManager.Instance.playerCamera.orthographicSize;
                started = true;
            }
        }
    }
}