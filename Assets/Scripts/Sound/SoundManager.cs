using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    /// <summary>
    /// Creates a sound using a prefab at the specified position. Will be destroyed after it finishes playing.
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public AudioSource CreateSoundAtPosition(GameObject prefab, Vector3 position)
    {
        GameObject placedSound = Instantiate(prefab, position, Quaternion.identity);
        AudioSource source = placedSound.GetComponent<AudioSource>();

        if (source == null)
        {
            Debug.LogError($"Audio prefab ({prefab.name}) provided does not have a AudioSource component!!!");
            return null;
        }
        
        if (!source.playOnAwake)
        {
            source.Play();
        }

        Destroy(placedSound, source.clip.length + 0.5f);

        return source;
    }

    /// <summary>
    /// Spawn a sound prefab with no position provided, will be played entirely 2D.
    /// </summary>
    /// <param name="prefab">Prefab with audio source component</param>
    public void CreateSound(GameObject prefab)
    {
        AudioSource source = CreateSoundAtPosition(prefab, Vector3.zero);

        if (source == null)
        {
            return;
        }
        
        source.spatialBlend = 0f;
    }
}
