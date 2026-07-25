using System;
using UnityEngine;

namespace Boss
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField]
        private float amount;
        [SerializeField]
        private float speed;

        private float direction = 1;

        public void Update()
        {
            float angle = transform.localRotation.eulerAngles.z + speed * direction * Time.deltaTime;
            if (Mathf.Abs(Fix(angle)) > amount)
            {
                direction *= -1;
            }
            transform.localRotation = Quaternion.Euler(0,0,angle);
        }

        private float Fix(float angle)
        {
            return angle < 180 ? angle : angle - 360;
        }
    }
}