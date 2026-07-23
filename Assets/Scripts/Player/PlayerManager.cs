using UnityEngine;

namespace Player
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;
        [SerializeField] public Camera playerCamera;

        public void Awake()
        {
            Instance = this;
        }
    }
}