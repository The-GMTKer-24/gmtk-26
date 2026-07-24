using System;
using System.Collections;
using Entity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class Player : MonoBehaviour
    {
        public static Player Instance;

        [SerializeField] private TimeEntity timeEntity;

        public TimeEntity TimeEntity => timeEntity;
        public PlayerModifier PlayerModifier => playerModifier;
        public SpriteRenderer BigBlackBox => bigBlackBox;
        [SerializeField] private SpriteRenderer bigBlackBox;
        [SerializeField] private PlayerModifier playerModifier;
        [SerializeField] private string mainMenu;
        [SerializeField] private float delay;
        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            timeEntity = GetComponent<TimeEntity>();
        }

        public void OnDestroy()
        {
            PlayerManager.Instance.LoadSceneAfterDelay(mainMenu, delay);
        }

    }
}