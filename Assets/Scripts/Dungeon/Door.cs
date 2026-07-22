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
        public void OpenDoor()
        {
            rend.sprite = open;
            col.enabled = false;
        }

        public void CloseDoor()
        {
            rend.sprite = closed;
            col.enabled = true;
        }

        public void HideDoor()// maybe disable and replace with wall?
        {
            rend.sprite = noDoor;
            col.enabled = true;
        }
    }
}
