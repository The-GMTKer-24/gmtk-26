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
        if (CheckIfInLos(other))
        {
            times.Add(other.gameObject.GetEntityId(),.5f);
        }
    }

    private bool CheckIfInLos(Collider2D other)
    {
        GameObject colliderGameObject = Physics2D.Raycast(transform.position,  transform.position - other.transform.position,~Physics.IgnoreRaycastLayer).collider?.gameObject;
        if (colliderGameObject == null) return false;
        print($"Checking los against {other.name} found {colliderGameObject.name}");
        return colliderGameObject ==
               other.gameObject;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (times.ContainsKey(other.gameObject.GetEntityId()))
        {
            if (!CheckIfInLos(other))
            {
                times.Remove(other.gameObject.GetEntityId());
                return;
            }
        }
        else
        {
            if (CheckIfInLos(other))
            {
                times.Add(other.gameObject.GetEntityId(), .5f);
            }
            else
            {
                return;
            }
        }


        EntityId id = other.gameObject.GetEntityId();
        times[id] -= Time.fixedDeltaTime;
        if (times[id] < 0)
        {
            TimeEntity timeEntity = other.gameObject.GetComponent<TimeEntity>();
            if (timeEntity != null)
            {
                timeEntity.DealDamage(DPS);
            }
            times[id] = 1f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        times.Remove(other.gameObject.GetEntityId());
    }
}
