using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public const int RegularEQ = 1;
    public const int DuckEQ = 0;
    
    public static MusicManager Instance;

    [SerializeField] private AudioMixerGroup mainMixer;
    [SerializeField] private float duckSpeed = 10f;

    private float currentDuckedAudioValue;
    private float mostRecentDuckedAudioValue;
    private AudioSource musicSource;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        musicSource = gameObject.GetComponent<AudioSource>();
        
        DontDestroyOnLoad(this);
    }

    void Update()
    {
        float duckedAudioValue = Mathf.Clamp(mostRecentDuckedAudioValue, 0f, 1f);
        currentDuckedAudioValue = Mathf.Lerp(currentDuckedAudioValue, duckedAudioValue, duckSpeed * Time.unscaledDeltaTime);
        mainMixer.audioMixer.SetFloat("eqDuck", currentDuckedAudioValue);
    }
    
    public void DuckEQToValue(float value)
    {
        mostRecentDuckedAudioValue = value;
    }

    public void DuckEQToLow()
    {
        mostRecentDuckedAudioValue = DuckEQ;
    }

    public void DuckEQToHigh()
    {
        mostRecentDuckedAudioValue = RegularEQ;
    }

    public void SetVolume(float value)
    {
        musicSource.volume = value;
    }
}
