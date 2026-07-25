using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    private Dictionary<GameObject, int> instances = new();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    private Dictionary<GameObject, int> activeSounds = new();

    [SerializeField] private int maxInstancesPerSound = 5;

    /// <summary>
    /// Creates a sound using a prefab at the specified position. Will be destroyed after it finishes playing.
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public AudioSource CreateSoundAtPosition(GameObject prefab, Vector3 position)
    {
        // Check current count
        if (activeSounds.TryGetValue(prefab, out int count))
        {
            if (count >= maxInstancesPerSound)
            {
                return null;
            }
        }
        else
        {
            activeSounds[prefab] = 0;
        }


        GameObject placedSound = Instantiate(prefab, position, Quaternion.identity);

        AudioSource source = placedSound.GetComponent<AudioSource>();

        if (source == null)
        {
            Debug.LogError($"Audio prefab ({prefab.name}) does not have an AudioSource component!");
            Destroy(placedSound);
            return null;
        }

        // Increment count
        activeSounds[prefab]++;

        if (!source.playOnAwake)
        {
            source.Play();
        }

        // Remove count when destroyed
        StartCoroutine(RemoveAfterPlaying(placedSound, prefab, source.clip.length));
        
        return source;
    }


    private IEnumerator RemoveAfterPlaying(GameObject soundObject, GameObject prefab, float delay)
    {
        yield return new WaitForSeconds(delay);

        activeSounds[prefab]--;
        Destroy(soundObject);
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
