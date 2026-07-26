using System.Collections;
using System.Collections.Generic;
using Attacks;
using Entity;
using UnityEngine;
using UnityEngine.Serialization;

public class TelegraphedLaserBeamAttack : MonoBehaviour, IAttack
{
    [SerializeField, Min(0f)] public float damage = 10f;
    [SerializeField, Min(0f)] public float range = 10f;
    [SerializeField, Min(0f)] public float staminaCost = 10f;
    [SerializeField, Min(0f)] public float timeCost = 10f;
    [SerializeField, Min(0f)] public float turnSpeed = 10f;

    [Header("Line Appearance")]
    [SerializeField] private AnimationCurve growthRate;
    [SerializeField, Min(0f)] private float width;
    [SerializeField] private AnimationCurve fadeInRate =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField, Min(0f)] private float telegraphFadeInDuration = 0.15f;
    [SerializeField, Min(0f)] private float beamFadeInDuration = 0.03f;

    [FormerlySerializedAs("directionalAnimation")]
    [SerializeField] private LineRenderer telegraphAnimation;

    [SerializeField] private LineRenderer beamAnimation;
    [SerializeField, Min(0.01f)] private float beamDuration = 0.1f;

    [Header("Wall Blocking")]
    [Tooltip("Only colliders on these layers shorten and block the laser.")]
    [SerializeField] private LayerMask wallMask;

    [Tooltip("Keeps the visible line slightly in front of the wall surface.")]
    [SerializeField, Min(0f)] private float wallEndPadding = 0.02f;

    [FormerlySerializedAs("delay")]
    [SerializeField, Min(0f)] public float attackDelay = 1f;

    private List<AttackInstance> _attackInstances;

    protected TimeEntity TimeEntity;
    protected StaminaEntity StaminaEntity;

    private void Awake()
    {
        _attackInstances = new List<AttackInstance>();
        TimeEntity = GetComponent<TimeEntity>();
        StaminaEntity = GetComponent<StaminaEntity>();

        if (TimeEntity == null && timeCost > 0f)
        {
            Debug.LogError(
                "TelegraphedLaserBeamAttack requires a TimeEntity when its time cost is nonzero.",
                this
            );
        }

        if (StaminaEntity == null && staminaCost > 0f)
        {
            Debug.LogError(
                "TelegraphedLaserBeamAttack requires a StaminaEntity when its stamina cost is nonzero.",
                this
            );
        }
    }

    public bool IsAoe()
    {
        return false;
    }

    public float GetDamage()
    {
        return Mathf.Max(0f, damage);
    }

    public float GetStaminaCost()
    {
        return Mathf.Max(0f, staminaCost);
    }

    public float GetTimeCost()
    {
        return Mathf.Max(0f, timeCost);
    }

    public float GetRange()
    {
        return Mathf.Max(0f, range);
    }

    public float GetDelay()
    {
        return Mathf.Max(0f, attackDelay);
    }

    public float CountFriendlyFires(Vector2 targetPosition)
    {
        if (GnomeTracker.Instance == null)
        {
            return 0f;
        }

        Vector2 origin = transform.position;
        Vector2 direction = targetPosition - origin;
        float attackRange = GetRange();

        if (direction.sqrMagnitude <= Mathf.Epsilon || attackRange <= 0f)
        {
            return 0f;
        }

        float unobstructedRange = GetWallDistance(
            origin,
            direction,
            attackRange,
            out _
        );

        if (unobstructedRange <= 0f)
        {
            return 0f;
        }

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            origin,
            direction.normalized,
            unobstructedRange
        );

        float friendlyFires = 0f;
        HashSet<EntityId> countedGnomes = new HashSet<EntityId>();

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }

            GnomeAI gnome = hit.collider.GetComponentInParent<GnomeAI>();

            if (gnome != null &&
                gnome.gameObject != gameObject &&
                countedGnomes.Add(gnome.gameObject.GetEntityId()))
            {
                friendlyFires += 1f;
            }
        }

        return friendlyFires;
    }

    public bool CanHit(Vector2 targetPosition)
    {
        Vector2 origin = transform.position;
        Vector2 toTarget = targetPosition - origin;
        float targetDistance = toTarget.magnitude;
        float attackRange = GetRange();

        if (targetDistance > attackRange)
        {
            return false;
        }

        if (targetDistance <= Mathf.Epsilon)
        {
            return true;
        }

        float wallDistance = GetWallDistance(
            origin,
            toTarget,
            targetDistance,
            out bool blocked
        );

        return !blocked || wallDistance + 0.001f >= targetDistance;
    }

    public float OutOfRangeDistance(Vector2 targetPosition)
    {
        float distance = Vector2.Distance(
            targetPosition,
            transform.position
        );

        return distance - GetRange();
    }

    private void Update()
    {
        foreach (AttackInstance instance in _attackInstances)
        {
            if (instance.Target != null)
            {
                Vector2 toTarget =
                    instance.Target.transform.position - transform.position;

                instance.DirectionController.Update(toTarget, Time.deltaTime);
            }

            UpdateTelegraph(instance);
        }
    }

    private void FixedUpdate()
    {
        for (int i = _attackInstances.Count - 1; i >= 0; i--)
        {
            AttackInstance instance = _attackInstances[i];
            instance.RemainingAttackTime -= Time.fixedDeltaTime;

            if (instance.RemainingAttackTime > 0f)
            {
                continue;
            }

            LaserAttack(instance);
            DestroyDisplay(instance);
            _attackInstances.RemoveAt(i);
        }
    }

    public void Attack(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (telegraphAnimation == null)
        {
            Debug.LogError(
                "TelegraphedLaserBeamAttack has no telegraph animation assigned.",
                this
            );
            return;
        }

        float safeTimeCost = GetTimeCost();
        float safeStaminaCost = GetStaminaCost();

        if (safeTimeCost > 0f && TimeEntity == null)
        {
            return;
        }

        if (safeStaminaCost > 0f &&
            (StaminaEntity == null ||
             !StaminaEntity.ConsumeStaminaIf(safeStaminaCost)))
        {
            return;
        }

        LineRenderer displayObject = Instantiate(
            telegraphAnimation,
            transform.position,
            Quaternion.identity
        );

        AttackInstance instance = new AttackInstance(
            gameObject,
            target,
            GetDelay(),
            displayObject,
            Mathf.Max(0f, turnSpeed)
        );

        _attackInstances.Add(instance);
        UpdateTelegraph(instance);
    }

    private void LaserAttack(AttackInstance instance)
    {
        Vector3 visualOrigin = transform.position;
        Vector2 physicsOrigin = visualOrigin;
        Vector2 direction = DirectionFromRotation(
            instance.DirectionController.direction
        );
        float attackRange = GetRange();

        float damageRange = GetWallDistance(
            physicsOrigin,
            direction,
            attackRange,
            out bool blocked
        );

        float visualRange = blocked
            ? Mathf.Max(0f, damageRange - wallEndPadding)
            : attackRange;

        float safeDamage = GetDamage();
        HashSet<EntityId> damagedEntities = new HashSet<EntityId>();

        if (damageRange > 0f)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                physicsOrigin,
                direction,
                damageRange
            );

            foreach (RaycastHit2D rayHit in hits)
            {
                if (rayHit.collider == null)
                {
                    continue;
                }

                TimeEntity targetTimeEntity =
                    AttackUtility.FindTimeEntity(rayHit.collider.gameObject);

                if (targetTimeEntity == null ||
                    targetTimeEntity == TimeEntity ||
                    !damagedEntities.Add(
                        targetTimeEntity.gameObject.GetEntityId()
                    ))
                {
                    continue;
                }

                if (safeDamage > 0f)
                {
                    targetTimeEntity.DealDamage(safeDamage);
                }
            }
        }

        CreateBeam(
            visualOrigin,
            direction,
            visualRange,
            instance.DirectionController.direction
        );

        float safeTimeCost = GetTimeCost();

        if (safeTimeCost > 0f && TimeEntity != null)
        {
            TimeEntity.DealDamage(safeTimeCost);
        }
    }

    private void UpdateTelegraph(AttackInstance instance)
    {
        if (instance.DisplayObject == null)
        {
            return;
        }

        float rotationDegrees = instance.DirectionController.direction;
        Vector2 direction = DirectionFromRotation(rotationDegrees);
        Vector2 origin = transform.position;
        float attackRange = GetRange();

        float wallDistance = GetWallDistance(
            origin,
            direction,
            attackRange,
            out bool blocked
        );

        float visualRange = blocked
            ? Mathf.Max(0f, wallDistance - wallEndPadding)
            : attackRange;

        instance.DisplayObject.transform.SetPositionAndRotation(
            transform.position,
            Quaternion.Euler(0f, 0f, rotationDegrees)
        );

        SetLinePositions(
            instance.DisplayObject,
            transform.position,
            direction,
            visualRange
        );

        float progress = instance.AttackDuration <= Mathf.Epsilon
            ? 1f
            : 1f - Mathf.Clamp01(
                instance.RemainingAttackTime / instance.AttackDuration
            );

        float elapsedTime = Mathf.Max(
            0f,
            instance.AttackDuration - instance.RemainingAttackTime
        );

        float fadeProgress = telegraphFadeInDuration <= Mathf.Epsilon
            ? 1f
            : Mathf.Clamp01(elapsedTime / telegraphFadeInDuration);

        instance.FadeState.Apply(
            instance.DisplayObject,
            EvaluateFade(fadeProgress)
        );

        if (width > 0f)
        {
            float widthScale = growthRate != null && growthRate.length > 0
                ? Mathf.Max(0f, growthRate.Evaluate(progress))
                : 1f;

            SetLineWidth(instance.DisplayObject, width * widthScale);
        }
    }

    private void CreateBeam(
        Vector3 origin,
        Vector2 direction,
        float attackRange,
        float rotationDegrees
    )
    {
        if (beamAnimation == null)
        {
            return;
        }

        LineRenderer beam = Instantiate(
            beamAnimation,
            origin,
            Quaternion.Euler(0f, 0f, rotationDegrees)
        );

        SetLinePositions(beam, origin, direction, attackRange);

        if (width > 0f)
        {
            SetLineWidth(beam, width);
        }

        float safeBeamDuration = Mathf.Max(0.01f, beamDuration);
        float safeFadeDuration = Mathf.Min(
            Mathf.Max(0f, beamFadeInDuration),
            safeBeamDuration
        );

        LineFadeState fadeState = new LineFadeState(beam);
        fadeState.Apply(beam, safeFadeDuration <= Mathf.Epsilon ? 1f : 0f);

        if (safeFadeDuration > Mathf.Epsilon)
        {
            StartCoroutine(FadeInBeam(beam, fadeState, safeFadeDuration));
        }

        Destroy(beam.gameObject, safeBeamDuration);
    }

    private IEnumerator FadeInBeam(
        LineRenderer beam,
        LineFadeState fadeState,
        float fadeDuration
    )
    {
        float elapsedTime = 0f;

        while (beam != null && elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeState.Apply(beam, EvaluateFade(progress));
            yield return null;
        }

        if (beam != null)
        {
            fadeState.Apply(beam, 1f);
        }
    }

    private float EvaluateFade(float progress)
    {
        float safeProgress = Mathf.Clamp01(progress);

        if (fadeInRate == null || fadeInRate.length == 0)
        {
            return safeProgress;
        }

        return Mathf.Clamp01(fadeInRate.Evaluate(safeProgress));
    }

    private float GetWallDistance(
        Vector2 origin,
        Vector2 direction,
        float maxDistance,
        out bool blocked
    )
    {
        blocked = false;

        float safeDistance = Mathf.Max(0f, maxDistance);

        if (safeDistance <= 0f ||
            direction.sqrMagnitude <= Mathf.Epsilon ||
            wallMask.value == 0)
        {
            return safeDistance;
        }

        RaycastHit2D wallHit = Physics2D.Raycast(
            origin,
            direction.normalized,
            safeDistance,
            wallMask
        );

        if (wallHit.collider == null)
        {
            return safeDistance;
        }

        blocked = true;
        return Mathf.Clamp(wallHit.distance, 0f, safeDistance);
    }

    private static void SetLinePositions(
        LineRenderer line,
        Vector3 origin,
        Vector2 direction,
        float length
    )
    {
        Vector2 safeDirection = direction.sqrMagnitude > Mathf.Epsilon
            ? direction.normalized
            : Vector2.up;

        line.useWorldSpace = true;
        line.positionCount = 2;
        line.SetPosition(0, origin);

        Vector3 end = origin + new Vector3(
            safeDirection.x,
            safeDirection.y,
            0f
        ) * Mathf.Max(0f, length);

        line.SetPosition(1, end);
    }

    private static void SetLineWidth(LineRenderer line, float lineWidth)
    {
        float safeWidth = Mathf.Max(0f, lineWidth);
        line.startWidth = safeWidth;
        line.endWidth = safeWidth;
    }

    private static Vector2 DirectionFromRotation(float rotationDegrees)
    {
        float radians = rotationDegrees * Mathf.Deg2Rad;

        return new Vector2(
            -Mathf.Sin(radians),
            Mathf.Cos(radians)
        );
    }

    private void OnDestroy()
    {
        if (_attackInstances == null)
        {
            return;
        }

        foreach (AttackInstance instance in _attackInstances)
        {
            DestroyDisplay(instance);
        }

        _attackInstances.Clear();
    }

    private void DestroyDisplay(AttackInstance instance)
    {
        if (instance.DisplayObject == null)
        {
            return;
        }

        Destroy(instance.DisplayObject.gameObject);
        instance.DisplayObject = null;
    }

    private sealed class LineFadeState
    {
        private readonly GradientColorKey[] _colorKeys;
        private readonly GradientAlphaKey[] _baseAlphaKeys;
        private readonly GradientAlphaKey[] _workingAlphaKeys;
        private readonly Gradient _workingGradient;

        public LineFadeState(LineRenderer line)
        {
            Gradient sourceGradient = line.colorGradient;
            _colorKeys = sourceGradient.colorKeys;
            _baseAlphaKeys = sourceGradient.alphaKeys;
            _workingAlphaKeys = new GradientAlphaKey[_baseAlphaKeys.Length];
            _workingGradient = new Gradient();
        }

        public void Apply(LineRenderer line, float opacity)
        {
            if (line == null)
            {
                return;
            }

            float safeOpacity = Mathf.Clamp01(opacity);

            for (int i = 0; i < _baseAlphaKeys.Length; i++)
            {
                _workingAlphaKeys[i] = new GradientAlphaKey(
                    _baseAlphaKeys[i].alpha * safeOpacity,
                    _baseAlphaKeys[i].time
                );
            }

            _workingGradient.SetKeys(_colorKeys, _workingAlphaKeys);
            line.colorGradient = _workingGradient;
        }
    }

    private class AttackInstance
    {
        public readonly GameObject Target;
        public readonly float AttackDuration;
        public readonly DirectionController DirectionController;
        public readonly LineFadeState FadeState;

        public LineRenderer DisplayObject;
        public float RemainingAttackTime;

        public AttackInstance(
            GameObject source,
            GameObject target,
            float attackWait,
            LineRenderer displayObject,
            float moveSpeed
        )
        {
            Target = target;
            DisplayObject = displayObject;
            AttackDuration = Mathf.Max(0f, attackWait);
            RemainingAttackTime = AttackDuration;
            FadeState = new LineFadeState(displayObject);
            FadeState.Apply(displayObject, 0f);

            Vector2 delta =
                target.transform.position - source.transform.position;

            DirectionController = new DirectionController(
                -Vector2.SignedAngle(delta, Vector2.up),
                Mathf.Max(0f, moveSpeed)
            );
        }
    }
}