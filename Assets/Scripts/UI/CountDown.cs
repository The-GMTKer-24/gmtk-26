using System;
using Entity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CountDown : MonoBehaviour
    {
        [SerializeField] private TimeEntity player;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Gradient gradient;
        
        [Header("Grow when time tick")]
        [SerializeField] private GameObject tickUpSound;
        [SerializeField] private GameObject tickDownSound;
        [SerializeField] private float resetClockSpeed = 7f;
        [SerializeField] private float growMultiplier = 1.1f;
        
        [Header("Ticking Arrow")]
        [SerializeField] private RectTransform tickingArrow;
        [SerializeField] private float tickToNewPositionSpeed = 10.0f;
        
        private float _currentArrowAngle = 0f;
        private int _currentSeconds = -1;
        private int _previousSeconds = -1;
        private Transform _parentUI;
        private Vector3 _parentUIOriginalScale;
        private bool ticked;
        
        private SoundManager _soundManager;
        
        
        public void Start()
        {
            _parentUI = transform.parent;
            _parentUIOriginalScale = _parentUI.localScale;
            _currentArrowAngle = timeToDegrees();
            
            _soundManager = SoundManager.Instance;
        }
        
        public void Update()
        {
            if (player)
            {
                text.color = gradient.Evaluate(1 - (player.GetTime() / player.GetMaxTime()));
                text.SetText(TimeSpan.FromSeconds(player.GetTime()).ToString("m\\:ss"));    
            }
            
            // Tick the clock
            _previousSeconds = _currentSeconds;
            _currentSeconds = (int)player.GetTime();
            if (_currentSeconds < _previousSeconds || _currentSeconds > _previousSeconds)
            {
                TickClock();
            }
            
            // Rotate arrow
            _currentArrowAngle = Mathf.Lerp(_currentArrowAngle, timeToDegrees(), tickToNewPositionSpeed * Time.unscaledDeltaTime);
            tickingArrow.localRotation = Quaternion.Euler(0f, 0f, _currentArrowAngle);
            
            // Lerp back to correct scaling
            _parentUI.localScale = Vector3.Lerp(_parentUI.localScale, _parentUIOriginalScale, resetClockSpeed * Time.unscaledDeltaTime);
        }

        private void TickClock()
        {
            _parentUI.localScale *= growMultiplier;

            _soundManager.CreateSound(ticked ? tickUpSound : tickDownSound);
            ticked = !ticked;
        }

        private float timeToDegrees()
        {
            return -6 * player.GetTime();
        }
        
        
    }
}