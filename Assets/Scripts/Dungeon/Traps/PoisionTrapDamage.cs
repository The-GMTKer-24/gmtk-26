using System;
using System.Collections.Generic;
using Entity;
using Unity.VisualScripting;
using UnityEngine;

public class PoisionTrapDamage : MonoBehaviour
{
    [SerializeField] private float DPS;
    private bool isRunningDamage;
    private float startTime = 0;
    private GnomeTracker gnomeTracker;

    private Dictionary<EntityId, float> times = new();

    private void Start()
    {
        gnomeTracker = GnomeTracker.Instance;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        print("New collision");
        times[other.gameObject.GetEntityId()] = .5f;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        print("Damaging someone in a poision cloud");
        EntityId id = other.gameObject.GetEntityId();
        times[id] -= Time.fixedDeltaTime;
        if (times[id] < 0)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                other.gameObject.GetComponent<TimeEntity>().DealDamage(DPS);
            }
            else
            {
                GnomeAI gnome = gnomeTracker.GetGnome(id);
                if (gnome is null)
                {
                    times.Remove(other.gameObject.GetEntityId());
                    return;
                }
                gnome.timeEntity.DealDamage(DPS);
            }

            times[id] = 1f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        times.Remove(other.gameObject.GetEntityId());
    }
}
