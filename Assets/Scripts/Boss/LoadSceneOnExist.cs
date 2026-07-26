using System;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Boss
{
    public class LoadSceneOnExist : MonoBehaviour
    {
        [SerializeField] private string scene;

        public void Awake()
        {
            PersistentData.Instance.BestTime =
                Math.Min(PersistentData.Instance.BestTime, PersistentData.Instance.CurrentTime);
            SceneManager.LoadScene(scene);
        }
    }
}