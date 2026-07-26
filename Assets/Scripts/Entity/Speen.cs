using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entity
{
    public class Speen : MonoBehaviour
    {
        public float speed;

        public void Update()
        {
            transform.rotation = Quaternion.Euler(0, 0, transform.rotation.z + speed * Time.deltaTime);
        }
    }
}