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
        
        
        [Header("Ticking Arrow")]
        [SerializeField] private RectTransform tickingArrow;
        [SerializeField] private float tickToNewPositionSpeed = 10.0f;
        
        private float _currentArrowAngle = 0f;

        public void Start()
        {
            _currentArrowAngle = timeToDegrees();
        }
        
        public void Update()
        {
            if (player)
            {
                text.color = gradient.Evaluate(1 - (player.GetTime() / player.GetMaxTime()));
                text.SetText(TimeSpan.FromSeconds(player.GetTime()).ToString("m\\:ss"));    
            }
            
            // Rotate arrow
            _currentArrowAngle = Mathf.Lerp(_currentArrowAngle, timeToDegrees(), tickToNewPositionSpeed * Time.unscaledDeltaTime);
            tickingArrow.localRotation = Quaternion.Euler(0f, 0f, _currentArrowAngle);
        }

        private float timeToDegrees()
        {
            return -6 * player.GetTime();
        }
    }
}