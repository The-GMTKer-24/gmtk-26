using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Cutscene
{
    [RequireComponent(typeof(Image))]
    public class CutsceneFrame : MonoBehaviour
    {
        [NonSerialized] public Image cutsceneImage;
        public float fadeDelay = 5;

        public void Awake()
        {
            cutsceneImage = GetComponent<Image>();
        }
    }
}