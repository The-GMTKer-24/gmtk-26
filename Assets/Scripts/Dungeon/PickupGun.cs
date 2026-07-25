using System;
using Player;
using UnityEngine;

public class PickupGun : MonoBehaviour
{
    [SerializeField] private GameObject pickupSound;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SoundManager.Instance.CreateSoundAtPosition(pickupSound, transform.position);
            WeaponManager.Instance.shotgunEnabled = true;
            Destroy(gameObject);
        }
    }
}
