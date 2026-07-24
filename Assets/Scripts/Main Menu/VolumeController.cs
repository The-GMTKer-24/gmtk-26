using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Main_Menu
{
    public class VolumeController : MonoBehaviour
    {
        [SerializeField] private AudioMixerGroup gameAudio;
        [SerializeField] private AudioMixerGroup musicAudio;

        [SerializeField] private Slider gameVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;

        public static float gameVolume = 1.0f;
        public static float musicVolume = 1.0f;

        private Slider slider;

        void Start()
        {
            gameVolume = PlayerPrefs.GetFloat("GameVolume", gameVolume);
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", musicVolume);

            gameVolumeSlider.value = gameVolume;
            musicVolumeSlider.value = musicVolume;
        }

        public void ChangeMusicVolume()
        {
            musicVolume = musicVolumeSlider.value;
            musicAudio.audioMixer.SetFloat("Volume", musicVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        }

        public void ChangeGameVolume()
        {
            gameVolume = gameVolumeSlider.value;
            gameAudio.audioMixer.SetFloat("Volume", gameVolume);
            PlayerPrefs.SetFloat("GameVolume", gameVolume);
        }

    }
}
