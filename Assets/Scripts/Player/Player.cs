using System;
using Entity;
using UnityEngine;

namespace Player
{
    public class Player : MonoBehaviour
    {
        public static Player Instance;

        [SerializeField] private TimeEntity timeEntity;

        public TimeEntity TimeEntity => timeEntity;
        
        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            timeEntity = GetComponent<TimeEntity>();
        }
    }
}