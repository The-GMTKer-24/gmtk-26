using UnityEngine;

namespace Dungeon
{
    public class Door : MonoBehaviour
    {
        [SerializeField] private Sprite open;
        [SerializeField] private Sprite closed;
        [SerializeField] private Sprite noDoor;

        [SerializeField] private SpriteRenderer rend;
        [SerializeField] private BoxCollider2D col;
        
        public bool Hidden { get; private set; }
        
        public void OpenDoor()
        {
            rend.sortingOrder = -1;
            rend.sprite = open;
            col.enabled = false;
            Hidden = false;
        }

        public void CloseDoor()
        {
            rend.sprite = closed;
            rend.sortingOrder = 2;
            col.enabled = true;
            Hidden = false;
        }

        public void HideDoor()// maybe disable and replace with wall?
        {
            rend.sprite = noDoor;
            col.enabled = true;
            Hidden = true;
        }
    }
}
