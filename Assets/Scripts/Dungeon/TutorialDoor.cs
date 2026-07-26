using System;
using Dungeon;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TutorialDoor : MonoBehaviour
{
    [SerializeField] private Door otherDoor;
    private ShadowCaster2D shadowCaster;
    [SerializeField] private SpriteRenderer rend;
    [SerializeField] private BoxCollider2D col;
    [SerializeField] private GameObject successSound;
    [SerializeField] private GameObject closeSound;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        shadowCaster = GetComponent<ShadowCaster2D>();
    }

    private void Start()
    {
        OpenDoors();
    }

    public bool Hidden { get; private set; }

    public void OpenDoors()
    {
        if (rend)
        {
            rend.sortingOrder = -1;
            rend.sprite = otherDoor.openHorizontal;
            col.enabled = false;
            Hidden = false;
            shadowCaster.enabled = false;
            otherDoor.OpenDoor();
        }
    }

    public void CloseDoors()
    {
        rend.sprite = otherDoor.closedHorizontal;
        rend.sortingOrder = 2;
        col.enabled = true;
        Hidden = false;
        shadowCaster.enabled = true;
        otherDoor.CloseDoor();
        SoundManager.Instance.CreateSound(closeSound);
    }

    public void GnomeDown()
    {
        OpenDoors();
        if (SoundManager.Instance)
            SoundManager.Instance.CreateSound(successSound);
    }
}
