using System;
using Entity;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Upgrades;

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
        [SerializeField] private float baseTickVolume = 0.1f;
        
        [Header("Grow when time tick")]
        [SerializeField] private GameObject tickUpSound;
        [SerializeField] private GameObject tickDownSound;
        [SerializeField] private float resetClockSpeed = 7f;
        [SerializeField] private float growMultiplier = 1.1f;
        [SerializeField] private float newBaseScaleWhenLosing = 2f;
        
        [Header("Ticking Arrow")]
        [SerializeField] private RectTransform tickingArrow;
        [SerializeField] private float tickToNewPositionSpeed = 10.0f;
        
        private float currentArrowAngle = 0f;

        private Transform parentUI;
        private Vector3 parentUIOriginalScale;
        private Vector3 currentParentUIScale;
        private bool ticked;

        private float vignetteIntensity = 1f;
        private Vignette vignette;

        private Vector3 startingPosition;
        
        private SoundManager soundManager;
        private MusicManager musicManager;
        
        public void Start()
        {
            parentUI = transform.parent;
            parentUIOriginalScale = parentUI.localScale;
            currentArrowAngle = timeToDegrees(player.GetTime());
            
            startingPosition = parentUI.localPosition;
            
            soundManager = SoundManager.Instance;
            musicManager = MusicManager.Instance;
            
            // get the vignette effect
            for (int i = 0; i < renderingVolume.components.Count; i++)
            {
                if (renderingVolume.components[i].name == "Vignette")
                {
                    vignette = (Vignette)renderingVolume.components[i];
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
            if (musicManager != null && !UIManager.Instance.Paused)
            {
                musicManager.DuckEQToValue(duckedAudioValue);
            }
            
            // Vignette
            vignetteIntensity = Mathf.Lerp(vignetteIntensity, duckedAudioValue, duckSpeed * Time.unscaledDeltaTime);
            vignette.intensity.value = (-1 * vignetteIntensity) + 1;
            
            if (time < almostDeadTime)
            {
                parentUI.localPosition = Vector3.Lerp(parentUI.localPosition, new Vector3(0, 0, transform.position.z), moveToCenterSpeed * Time.deltaTime);
                
                currentParentUIScale = Vector3.Lerp(currentParentUIScale, new Vector3(newBaseScaleWhenLosing * parentUIOriginalScale.x, newBaseScaleWhenLosing * parentUIOriginalScale.y, parentUIOriginalScale.z), moveToCenterSpeed * Time.deltaTime);
            }
            else
            {
                parentUI.localPosition = Vector3.Lerp(parentUI.localPosition, startingPosition, returnFromCenterSpeed * Time.deltaTime);
                
                currentParentUIScale = Vector3.Lerp(currentParentUIScale, parentUIOriginalScale, returnFromCenterSpeed * Time.deltaTime);
            }
            
            // Rotate arrow
            currentArrowAngle = Mathf.Lerp(currentArrowAngle, timeToDegrees(time), tickToNewPositionSpeed * Time.unscaledDeltaTime);
            tickingArrow.localRotation = Quaternion.Euler(0f, 0f, currentArrowAngle);
            
            // Lerp back to correct scaling
            parentUI.localScale = Vector3.Lerp(parentUI.localScale, currentParentUIScale, resetClockSpeed * Time.unscaledDeltaTime);
        }

        public void TickSound()
        {
            soundManager.CreateSound(ticked ? tickUpSound : tickDownSound);
            if (FatalTempo.Instance)
            {
                FatalTempo.Instance.Tick();
            }
            ticked = !ticked;
        }

        // Grows the clock to then shrink back down
        public void TickPulse()
        {
            parentUI.localScale *= growMultiplier;
        }

        private float timeToDegrees(float time)
        {
            return -6 * time;
        }
    }
}