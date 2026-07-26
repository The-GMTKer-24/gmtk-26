using Attacks;
using Entity;
using UnityEngine;

public class SelfExplosion : GenericAttack, IAttack
{
    [SerializeField] private GameObject explosion;
    [SerializeField] private GameObject explosionSound;
    [SerializeField] private GameObject boomGnomeAttack;
    [SerializeField, Min(0f)] private float boomDelay = .5f;

    private bool _isExploding;
    private bool _hasExploded;
    
    public override bool IsAoe()
    {
        return true;
    }

    public override float GetDelay()
    {
        return Mathf.Max(0f, boomDelay);
    }
    
    public override bool CanHit(Vector2 targetPosition)
    {
        if (_isExploding)
        {
            return false;
        }

        float distanceSq = Vector2.SqrMagnitude(targetPosition - (Vector2)transform.position);
        float attackRange = GetRange();
        
        return distanceSq <= attackRange * attackRange;
    }
    
    public override float OutOfRangeDistance(Vector2 targetPosition)
    {
        float distance = Vector2.Distance(targetPosition, (Vector2)transform.position);
        
        return distance - GetRange();
    }

    public override float CountFriendlyFires(Vector2 targetPosition)
    {
        float hits = 0f;

        if (GnomeTracker.Instance == null)
        {
            return hits;
        }

        foreach (GnomeAI gnomeAI in GnomeTracker.Instance.GetGnomeEnumerator())
        {
            if (gnomeAI != null &&
                gnomeAI.gameObject != gameObject &&
                CanHit(gnomeAI.transform.position))
            {
                hits += 1f;
            }
        }
        
        return hits;
    }

    public override void Attack(GameObject desiredTarget)
    {
        if (_isExploding || !TryConsumeStaminaCost())
        {
            return;
        }

        _isExploding = true;
        
        if (boomGnomeAttack != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.CreateSoundAtPosition(
                boomGnomeAttack,
                transform.position
            );
        }

        float safeDelay = Mathf.Max(0f, boomDelay);

        if (safeDelay <= 0f)
        {
            Explode();
        }
        else
        {
            Invoke(nameof(Explode), safeDelay);
        }
    }

    private void Explode()
    {
        if (!_isExploding || _hasExploded)
        {
            return;
        }

        _hasExploded = true;

        foreach (GameObject hit in GetAllInRange())
        {
            TimeEntity targetTimeEntity = AttackUtility.FindTimeEntity(hit);

            if (targetTimeEntity == null)
            {
                continue;
            }

            float safeDamage = GetDamage();

            if (safeDamage > 0f)
            {
                targetTimeEntity.DealDamage(safeDamage);
            }
        }

        if (explosion != null)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
        }

        if (explosionSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.CreateSoundAtPosition(
                explosionSound,
                transform.position
            );
        }

        ApplyTimeCost();
        Destroy(gameObject);
    }
}
