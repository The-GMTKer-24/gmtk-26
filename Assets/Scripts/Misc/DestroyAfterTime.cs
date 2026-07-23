using System;
using UnityEngine;

namespace Misc
{
    public class DestroyAfterTime : MonoBehaviour
    {
        [SerializeField] private float time;
        private float elapsed;

        public void Update()
        {
            elapsed += Time.deltaTime;
            if (elapsed >= time)
            {
                Destroy(gameObject);
            }
        }
    }
}