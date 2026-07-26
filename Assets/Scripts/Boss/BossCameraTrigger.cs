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

        private Camera camera;
        private CameraFollow cameraFollow;
        
        void Awake()
        {
            camera = Camera.main;
            if (camera != null)
            {
                cameraFollow = camera.GetComponent<CameraFollow>();
                if (cameraFollow == null)
                {
                    Debug.LogError("Where the fuck is the camera follow script");
                }
            }
        }
        
        public void Update()
        {
            if (started)
            {
                camera.orthographicSize = Mathf.Lerp(startSize, size, sizeCurve.Evaluate(progress / timeToSize));
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
                cameraFollow.target = newTarget;
                cameraFollow.DisableLerping = true;
                startSize = camera.orthographicSize;
                started = true;
            }
        }
    }
}