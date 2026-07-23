using System;
using Player;
using UnityEngine;

namespace Entity
{
    
    public class TimeCollectable : MonoBehaviour
    {
        [SerializeField] private AnimationCurve popout;
        [SerializeField] private AnimationCurve popin;
        [SerializeField] private float scale;
        [SerializeField] private float amount;
        [SerializeField] private float growTime;
        [SerializeField] private float shrinkTime;
        private float growTimer;
        private float shrinkTimer;
        private bool shrinking;

        private void Awake()
        {
            shrinking = false;
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                print("Hit player");
                Player.Player.Instance.TimeEntity.Heal(amount);
                shrinking = true;
            }
        }

        public void Update()
        {
            if (growTimer < growTime)
            {
                growTimer += Time.deltaTime;
                if (growTimer < growTime)
                {
                    var s = popin.Evaluate(growTimer / growTime);
                    transform.localScale = new Vector3(s,s,s) * scale;
                }
            }
            if (shrinking)
            {
                print("shrinking");
                shrinkTimer += Time.deltaTime;
                var s = popout.Evaluate(shrinkTimer / shrinkTime);
                transform.localScale = new Vector3(s, s, s) * scale;
                if (shrinkTimer > 1)
                {
                    Destroy(this);
                }
            }
        }
    }
}