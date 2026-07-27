using UnityEngine;
using UnityEngine.SceneManagement;

public class EasymodeSpawner : MonoBehaviour
{

    [SerializeField] private GameObject easyModeObject;

    private void Awake()
    {
        if (PlayerPrefs.GetInt("EasyMode") > 0)
        {
            Instantiate(easyModeObject);
        }
    }
}
