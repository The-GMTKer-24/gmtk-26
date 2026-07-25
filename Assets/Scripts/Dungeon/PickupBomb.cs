using System;
using Player;
using UnityEngine;

public class PickupBomb : MonoBehaviour
{
    [SerializeField] private GameObject pickupSound;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SoundManager.Instance.CreateSoundAtPosition(pickupSound, transform.position);
            WeaponManager.Instance.bombEnabled = true;
            Destroy(gameObject);
        }
    }
}
