using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Dungeon
{
    public class Door : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] public Sprite openHorizontal;
        [SerializeField] public Sprite closedHorizontal;
        [SerializeField] public Sprite blockedHorizontal;
        [SerializeField] public Sprite openVertical;
        [SerializeField] public Sprite closedVertical;
        [SerializeField] public Sprite blockedVertical;

        
        [Header("Others")]
        [SerializeField] private SpriteRenderer rend;
        [SerializeField] private BoxCollider2D col;

        [SerializeField] private DoorDirections direction;

        enum DoorDirections
        {
            Vertical,
            Horizontal
        }

        private ShadowCaster2D shadowCaster;

        private void Awake()
        {
            shadowCaster = GetComponent<ShadowCaster2D>();
        }

        public bool Hidden { get; private set; }
        
        public void OpenDoor()
        {
            rend.sortingOrder = -1;
            if (direction == DoorDirections.Vertical)
            {
                rend.sprite = openVertical;
            }
            else
            {
                rend.sprite = openHorizontal;
            }
            col.enabled = false;
            Hidden = false;
            shadowCaster.enabled = false;
        }

        public void CloseDoor()
        {
            if (direction == DoorDirections.Vertical)
            {
                rend.sprite = closedVertical;
            }
            else
            {
                rend.sprite = closedHorizontal;
            }
            rend.sortingOrder = 2;
            col.enabled = true;
            Hidden = false;
            shadowCaster.enabled = true;
        }

        public void HideDoor()// maybe disable and replace with wall?
        {
            
            if (direction == DoorDirections.Vertical)
            {
                rend.sprite = blockedVertical;
            }
            else
            {
                rend.sprite = blockedHorizontal;
            }
            col.enabled = true;
            Hidden = true;
            if (shadowCaster)
            {
                shadowCaster.enabled = true;
            }
        }
    }
}
