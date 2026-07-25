using System;
using Entity;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace UI
{
    public class CountDown : MonoBehaviour
    {
        
        [SerializeField] private TimeEntity player;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Gradient gradient;
        
        [Header("Audio Duck when time low")]
        [SerializeField] private VolumeProfile renderingVolume;
        [SerializeField] private AudioMixerGroup mainMixer;
        [SerializeField] private int timeLowWhenLessThanThis = 10;
        [SerializeField] private int almostDeadTime = 3;
        [SerializeField] private float duckSpeed = 10f;
        [SerializeField] private float moveToCenterSpeed = 0.02f;
        [SerializeField] private float returnFromCenterSpeed = 10f;
        
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

        private int _soundSeconds = -1;
        private int _previousSoundSeconds = -1;
        
        private float _soundTimer = 0f;
        private Transform _parentUI;
        private Vector3 _parentUIOriginalScale;
        private bool _ticked;

        private float _currentDuckedAudioValue = 1f;
        private Vignette _vignette;

        private Vector3 _startingPosition;
        
        private SoundManager _soundManager;
        
        public void Start()
        {
            _parentUI = transform.parent;
            _parentUIOriginalScale = _parentUI.localScale;
            _currentArrowAngle = timeToDegrees(player.GetTime());
            
            _startingPosition = _parentUI.localPosition;
            
            _soundManager = SoundManager.Instance;
            
            // get the vignette effect
            for (int i = 0; i < renderingVolume.components.Count; i++)
            {
                if (renderingVolume.components[i].name == "Vignette")
                {
                    _vignette = (Vignette)renderingVolume.components[i];
                }
            }
        }
        
        public void Update()
        {
            // Clamped time so it doesn't go negative
            float time = Mathf.Clamp(player.GetTime(), 0, player.GetMaxTime());

            if (player)
            {
                text.color = gradient.Evaluate(1 - (player.GetTime() / player.GetMaxTime()));
                text.SetText(TimeSpan.FromSeconds(time).ToString("m\\:ss"));    
            }

            // Duck the EQ when the time is low
            float duckedAudioValue = Mathf.Clamp(time / timeLowWhenLessThanThis, 0f, 1f);
            _currentDuckedAudioValue = Mathf.Lerp(_currentDuckedAudioValue, duckedAudioValue, duckSpeed * Time.unscaledDeltaTime);
            mainMixer.audioMixer.SetFloat("eqDuck", _currentDuckedAudioValue);
            _vignette.intensity.value = (-1 * _currentDuckedAudioValue) + 1;
            
            if (time < almostDeadTime)
            {
                _parentUI.localPosition = Vector3.Lerp(_parentUI.localPosition, new Vector3(0, 0, transform.position.z), moveToCenterSpeed * Time.deltaTime);
            }
            else
            {
                _parentUI.localPosition = Vector3.Lerp(_parentUI.localPosition, _startingPosition, returnFromCenterSpeed * Time.deltaTime);
            }
            
            // Rotate arrow
            _currentArrowAngle = Mathf.Lerp(_currentArrowAngle, timeToDegrees(time), tickToNewPositionSpeed * Time.unscaledDeltaTime);
            tickingArrow.localRotation = Quaternion.Euler(0f, 0f, _currentArrowAngle);
            
            // Lerp back to correct scaling
            _parentUI.localScale = Vector3.Lerp(_parentUI.localScale, _parentUIOriginalScale, resetClockSpeed * Time.unscaledDeltaTime);
        }

        public void TickSound()
        {
            _soundManager.CreateSound(_ticked ? tickUpSound : tickDownSound);
            _ticked = !_ticked;
        }

        // Grows the clock to then shrink back down
        public void TickPulse()
        {
            _parentUI.localScale *= growMultiplier;
        }

        private float timeToDegrees(float time)
        {
            return -6 * time;
        }
    }
}