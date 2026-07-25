using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Boss
{
    public class LoadSceneOnExist : MonoBehaviour
    {
        [SerializeField] private string scene;

        public void Awake()
        {
            SceneManager.LoadScene(scene);
        }
    }
}