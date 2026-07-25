using System;
using UnityEngine;

public class TutorialGnome : MonoBehaviour
{
    public TutorialDoor callbackDoor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnDestroy()
    {
        callbackDoor.GnomeDown();
    }
}
