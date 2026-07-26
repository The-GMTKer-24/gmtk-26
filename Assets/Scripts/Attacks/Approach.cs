using System;
using UnityEngine;

namespace Attacks
{
    public class Approach : MonoBehaviour
    {
        [SerializeField] public Rigidbody2D rb;
        [SerializeField] public float range;
        [SerializeField] public float speed;

        public void Update()
        {
            Vector2 toPlayer = (Vector2)Player.Player.Instance.gameObject.transform.position - (Vector2)gameObject.transform.position;
            float signedApproachStrength =
                (Vector2.Distance((Vector2)Player.Player.Instance.gameObject.transform.position, (Vector2)gameObject.transform.position) / range - 0.8f) / 0.4f;
            rb.linearVelocity = (toPlayer.normalized * (Mathf.Sign(signedApproachStrength) * Mathf.Clamp01(Mathf.Abs(signedApproachStrength)))).normalized * speed;
        }
    }
}