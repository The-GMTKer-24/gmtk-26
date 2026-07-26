using Attacks;
using Entity;
using Player;
using UnityEngine;

public class BulletAttack : GenericAttack, IAttack
{
    [SerializeField, Min(0.01f)]
    public float speed = 10f;

    [SerializeField]
    public GameObject bulletPrefab;

    [SerializeField, Min(0f)]
    public float spawnOffset = 0.6f;

    public override bool IsAoe()
    {
        return false;
    }

    public override bool CanHit(Vector2 targetPosition)
    {
        Vector2 origin = transform.position;
        Vector2 toTarget = targetPosition - origin;
        float attackRange = GetRange();

        if (toTarget.sqrMagnitude > attackRange * attackRange)
        {
            return false;
        }

        if (!TryGetFirstHit(targetPosition, out RaycastHit2D rayHit))
        {
            return false;
        }

        if (Player.Player.Instance == null)
        {
            return false;
        }

        GameObject hitObject = GetHitObject(rayHit);

        if (hitObject == null)
        {
            return false;
        }

        return object.Equals(
            Player.Player.Instance.gameObject.GetEntityId(),
            hitObject.GetEntityId()
        );
    }

    public override float OutOfRangeDistance(Vector2 targetPosition)
    {
        return Vector2.Distance(targetPosition, transform.position) - GetRange();
    }

    public override float CountFriendlyFires(Vector2 targetPosition)
    {
        if (!TryGetFirstHit(targetPosition, out RaycastHit2D rayHit))
        {
            return 0f;
        }

        GameObject hitObject = GetHitObject(rayHit);

        if (hitObject == null || GnomeTracker.Instance == null)
        {
            return 0f;
        }

        return GnomeTracker.Instance.DoesGnomeExist(hitObject.GetEntityId())
            ? 1f
            : 0f;
    }

    public override void Attack(GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning("BulletAttack was given a null target.", this);
            return;
        }

        if (bulletPrefab == null)
        {
            Debug.LogError("BulletAttack has no bullet prefab assigned.", this);
            return;
        }

        if (speed <= 0f)
        {
            Debug.LogError("BulletAttack speed must be greater than zero.", this);
            return;
        }

        if (bulletPrefab.GetComponent<EnemyBullet>() == null)
        {
            Debug.LogError(
                "The assigned bullet prefab does not contain an EnemyBullet component.",
                bulletPrefab
            );
            return;
        }

        Vector2 origin = transform.position;
        Vector2 toTarget = (Vector2)target.transform.position - origin;

        if (toTarget.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        if (!TryConsumeStaminaCost())
        {
            return;
        }

        Vector2 direction = toTarget.normalized;
        float safeSpawnOffset = Mathf.Min(
            Mathf.Max(0f, spawnOffset),
            GetRange()
        );
        Vector2 spawnPosition = origin + direction * safeSpawnOffset;

        // Assumes the bullet sprite points upward at rotation 0.
        float rotation = -Vector2.SignedAngle(direction, Vector2.up);

        GameObject bullet = Instantiate(
            bulletPrefab,
            spawnPosition,
            Quaternion.Euler(0f, 0f, rotation)
        );

        EnemyBullet enemyBullet = bullet.GetComponent<EnemyBullet>();

        if (enemyBullet == null)
        {
            Debug.LogError(
                "The assigned bullet prefab does not contain an EnemyBullet component.",
                bullet
            );

            Destroy(bullet);
            return;
        }

        enemyBullet.velocity = speed * direction;
        enemyBullet.damage = GetDamage();
        enemyBullet.remainingTime =
            Mathf.Max(0f, GetRange() - safeSpawnOffset) / speed;

        Rigidbody2D bulletBody = bullet.GetComponent<Rigidbody2D>();

        if (bulletBody != null)
        {
            bulletBody.rotation = rotation;
        }

        ApplyTimeCost();
    }

    /// <summary>
    /// Gets the closest collider between this attack and the requested
    /// position, ignoring colliders belonging to the attacker.
    /// </summary>
    private bool TryGetFirstHit(
        Vector2 targetPosition,
        out RaycastHit2D closestHit
    )
    {
        closestHit = default;

        Vector2 origin = transform.position;
        Vector2 toTarget = targetPosition - origin;
        float targetDistance = toTarget.magnitude;

        if (targetDistance <= Mathf.Epsilon)
        {
            return false;
        }

        Vector2 direction = toTarget / targetDistance;
        float rayDistance = Mathf.Min(targetDistance, GetRange());

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            origin,
            direction,
            rayDistance
        );

        Rigidbody2D ownerBody = GetComponentInParent<Rigidbody2D>();

        bool foundHit = false;
        float closestDistance = float.PositiveInfinity;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }

            // Ignore the attacker's own Rigidbody2D.
            if (ownerBody != null && hit.rigidbody == ownerBody)
            {
                continue;
            }

            // Ignore colliders directly within this object's hierarchy.
            Transform hitTransform = hit.collider.transform;

            if (hitTransform == transform ||
                hitTransform.IsChildOf(transform) ||
                transform.IsChildOf(hitTransform))
            {
                continue;
            }

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestHit = hit;
                foundHit = true;
            }
        }

        return foundHit;
    }

    private static GameObject GetHitObject(RaycastHit2D hit)
    {
        // Prefer the Rigidbody object because colliders are commonly
        // placed on child objects of an entity.
        if (hit.rigidbody != null)
        {
            return hit.rigidbody.gameObject;
        }

        return hit.collider != null
            ? hit.collider.gameObject
            : null;
    }
}
