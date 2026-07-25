using System;
using System.Collections;
using Entity;
using UI;
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
        public Rigidbody2D RigidBody { get; private set; }

        [SerializeField] private SpriteRenderer bigBlackBox;
        [SerializeField] private PlayerModifier playerModifier;
        [SerializeField] private string mainMenu;
        [SerializeField] private float delay;
        [SerializeField] private CountDown countdownClock;
        
        private int _currentSecond = -1;
        private int _previousSecond = -1;
        private float _currentTime = 0;
        
        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            timeEntity = GetComponent<TimeEntity>();
            RigidBody = GetComponent<Rigidbody2D>();
            
            _currentTime = timeEntity.GetTime();
        }

        public void OnDestroy()
        {
            PlayerManager.Instance.LoadSceneAfterDelay(mainMenu, delay);
            
            countdownClock.TickPulse();
            countdownClock.TickSound();
        }

        public void FixedUpdate()
        {
            TickPulsingAndSound();
        }

        private void TickPulsingAndSound()
        {
            // Separate clock
            _currentTime -= Time.fixedDeltaTime;
            
            // Keep track in seconds
            _previousSecond = _currentSecond;
            _currentSecond = (int)timeEntity.GetTime();
            
            // New change in a whole second!
            if (_previousSecond != _currentSecond)
            {
                // Pulse!
                countdownClock.TickPulse();
                
                // If this clock and the other clock are different, sync them
                // and avoid playing a sound, since it isn't a natural tick
                if ((int)_currentTime != _currentSecond)
                {
                    _currentTime = timeEntity.GetTime();
                    return;
                }
                
                // Otherwise, play a sound!
                countdownClock.TickSound();
            }
        }

    }
}