using System.Collections.Generic;
using Attacks;
using Entity;
using UnityEngine;
using UnityEngine.Serialization;

public class GnomeAI : MonoBehaviour
{
    private static readonly int Up = Animator.StringToHash("Up");
    private static readonly int Down = Animator.StringToHash("Down");
    private static readonly int Left = Animator.StringToHash("Left");
    private static readonly int Right = Animator.StringToHash("Right");

    [SerializeField] private AttackContainer attackContainer;
    
    [Header("Movement and Animation")]
    [SerializeField] public float animationSpeed = 1f;
    [SerializeField, Min(0f)] public float speed = 2f;
    [SerializeField, Min(0f)] public float forceDropoff = 0.2f;
    [SerializeField, Min(0f)] private float animationMovementThreshold = 0.05f;
    [SerializeField, Min(0f)] private float directionHysteresis = 0.08f;
    [SerializeField, Min(0f)] private float dispersionRadius = 5f;

    [Header("AI Update Rates")]
    [Tooltip("How often the gnome re-evaluates every available attack.")]
    [SerializeField, Min(0.02f)] private float decisionInterval = 0.15f;

    //[Tooltip("How much better a different attack must be before the gnome switches to it.")]
    //[SerializeField] private float attackSwitchThreshold = 0.1f;

    [Tooltip("Minimum time between calls to Attack().")]
    [SerializeField, Min(0.02f)] private float minimumAttackInterval = 0.25f;

    [Tooltip("How often nearby gnomes are checked for crowd separation.")]
    [SerializeField, Min(0.02f)] private float crowdCheckInterval = 0.4f;

    [Header("Attack Preferences")]
    [SerializeField] public bool canRepeatAttacks;

    [SerializeField]
    [Range(-1f, 1f)]
    public float timeConservationPreference = 0f;

    [SerializeField]
    [Range(0f, 1f)]
    public float selfishness = 0.1f;

    [FormerlySerializedAs("outOfRangeLoss")]
    [SerializeField] public float outOfRangeLossPerUnit = 1f;
    [SerializeField, Min(0f)] public float telegraphLoss = 1f;
    [SerializeField, Min(0f)] public float desperationTimeCutoff = 5f;
    [SerializeField, Min(0f)] public float desperationInstantAttackPreference = 10f;
    [SerializeField, Min(0f)] public float constantDecrowdingStrength = 0.1f;
    [SerializeField, Min(0f)] public float aoeDecrowdingStrength = 0.5f;

    [Header("Debug")]
    [SerializeField] public float currentLoss;

    [SerializeField] public MonoBehaviour chosenAttackType;

    public TimeEntity timeEntity;
    public StaminaEntity staminaEntity;

    private GameObject _player;
    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private SortingOrderHandler _sortingOrderHandler;
    private IAttack _previousAttack;
    private IAttack _chosenAttack;
    private readonly List<IAttack> _attacks = new List<IAttack>();

    private float _nextAttackDecisionTime;
    private float _nextMoveDecisionTime;
    private float _nextAttackTime;
    private float _nextCrowdCheckTime;

    private Vector2 _cachedDisperseVector;
    private FacingDirection _facingDirection = FacingDirection.Down;
    private FacingDirection? _appliedFacingDirection;

    public int GetSortingOrder()
    {
        if (_sortingOrderHandler != null)
        {
            return Mathf.RoundToInt(transform.position.y * -100f);
        }

        return _spriteRenderer != null
            ? _spriteRenderer.sortingOrder
            : Mathf.RoundToInt(transform.position.y * -100f);
    }

    private enum FacingDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    private static bool IsAttackAvailable(IAttack attack)
    {
        if (attack == null)
        {
            return false;
        }

        return attack is not UnityEngine.Object unityObject ||
               unityObject != null;
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _sortingOrderHandler = GetComponent<SortingOrderHandler>();
        attackContainer ??= GetComponent<AttackContainer>();
        timeEntity ??= GetComponent<TimeEntity>();
        staminaEntity ??= GetComponent<StaminaEntity>();

        if (_rb == null ||
            _animator == null ||
            _spriteRenderer == null ||
            timeEntity == null ||
            staminaEntity == null ||
            GnomeTracker.Instance == null)
        {
            Debug.LogError(
                "GnomeAI is missing a required attack, physics, animation, entity, or tracker reference.",
                this
            );
            enabled = false;
            return;
        }

        CacheAttacks();

        if (_attacks.Count == 0)
        {
            Debug.LogError(
                "GnomeAI could not find any usable IAttack components.",
                this
            );
            enabled = false;
            return;
        }

        if (Player.Player.Instance == null)
        {
            Debug.LogError("GnomeAI could not find the player.", this);
            enabled = false;
            return;
        }

        _player = Player.Player.Instance.gameObject;
        _previousAttack = null;
        
        _animator.speed = Mathf.Max(0f, animationSpeed);
        ApplyFacingDirection();

        GnomeTracker.Instance.AddGnome(this);
    }

    private void CacheAttacks()
    {
        _attacks.Clear();

        if (attackContainer != null)
        {
            foreach (IAttack attack in attackContainer.GetAttacks())
            {
                AddAttackIfAvailable(attack);
            }
        }

        if (_attacks.Count > 0)
        {
            return;
        }

        // Some existing gnome prefabs predate AttackContainer and place
        // their IAttack components directly on this GameObject.
        foreach (MonoBehaviour component in GetComponents<MonoBehaviour>())
        {
            if (component is IAttack attack)
            {
                AddAttackIfAvailable(attack);
            }
        }
    }

    private void AddAttackIfAvailable(IAttack attack)
    {
        if (IsAttackAvailable(attack) && !_attacks.Contains(attack))
        {
            _attacks.Add(attack);
        }
    }

    private void Update()
    {
        if (IsAttackAvailable(_chosenAttack) &&
            _chosenAttack is MonoBehaviour chosenAttackMB)
        {
            chosenAttackType = chosenAttackMB;
        }
        else
        {
            chosenAttackType = null;
        }

        if (_sortingOrderHandler == null)
        {
            _spriteRenderer.sortingOrder =
                Mathf.RoundToInt(transform.position.y * -100f);
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (!_player)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 playerPosition = _player.transform.position;
        //print("A: " + playerPosition);

        if (Time.fixedTime >= _nextAttackDecisionTime)
        {
            _chosenAttack = ChooseAttack(playerPosition);
            _nextAttackDecisionTime = Time.fixedTime + Mathf.Max(0.02f, decisionInterval);
        }

        TryAttack(_chosenAttack, playerPosition);

        if (Time.fixedTime >= _nextMoveDecisionTime)
        {
            MoveForAttack(_chosenAttack, playerPosition);
            _nextMoveDecisionTime = Time.fixedTime + Mathf.Max(0.02f, decisionInterval);
        }
    }

    private IAttack ChooseAttack(Vector2 playerPosition)
    {
        IAttack bestAttack = null;
        float bestLoss = float.PositiveInfinity;
        
        foreach (IAttack attack in _attacks)
        {
            if (!IsAttackAvailable(attack))
            {
                continue;
            }

            if (!canRepeatAttacks && attack == _previousAttack)
            {
                continue;
            }

            float loss = EvaluateAttackLoss(attack, playerPosition);

            if (loss < bestLoss)
            {
                bestAttack = attack;
                bestLoss = loss;
            }
        }

        if (bestAttack == null)
        {
            // A single configured attack is still preferable to crashing when
            // repeat prevention has no alternative to choose.
            if (IsAttackAvailable(_previousAttack))
            {
                bestAttack = _previousAttack;
                bestLoss = EvaluateAttackLoss(bestAttack, playerPosition);
            }
            else
            {
                currentLoss = float.PositiveInfinity;
                return null;
            }
        }

        currentLoss = bestLoss;
        _nextCrowdCheckTime = 0f;
        return bestAttack;
    }

    private float EvaluateAttackLoss(IAttack attack, Vector2 playerPosition)
    {
        float timeGain = Mathf.Clamp01((timeConservationPreference + 1f) * 0.5f);
        float staminaGain = 1f - timeGain;

        float remainingTime = timeEntity.GetTime();
        bool desperate = desperationTimeCutoff > 0f &&
                         remainingTime <= desperationTimeCutoff;

        float desperation = desperate
            ? 1f - Mathf.Clamp01(remainingTime / desperationTimeCutoff)
            : 0f;

        float friendlyFireCount = Mathf.Max(
            0f,
            attack.CountFriendlyFires(playerPosition)
        );
        bool canHit = attack.CanHit(playerPosition);

        float damage = Mathf.Max(attack.GetDamage(), 0.001f);
        float staminaCost = Mathf.Max(attack.GetStaminaCost(), 0f);
        float timeCost = Mathf.Max(attack.GetTimeCost(), 0f);
        float delay = Mathf.Max(attack.GetDelay(), 0f);

        float friendlyFireLoss =
            friendlyFireCount * (1f - Mathf.Clamp01(selfishness));

        float effectiveTimeCost =
            Mathf.Min(timeCost, Mathf.Max(remainingTime, 0f));
        

        float staminaLoss = staminaCost / damage;
        float timeLoss = (friendlyFireLoss + effectiveTimeCost) / damage;

        float totalLoss =
            staminaLoss * staminaGain +
            timeLoss * timeGain;
        
        if (!canHit)
        {
            totalLoss +=
                Mathf.Max(0f, outOfRangeLossPerUnit) *
                Mathf.Max(
                    0f,
                    attack.OutOfRangeDistance(playerPosition)
                );
        }

        totalLoss += delay * Mathf.Max(0f, telegraphLoss);

        if (desperate)
        {
            bool canAttackNow =
                staminaCost <= staminaEntity.GetStamina() &&
                canHit;

            float desperateLoss =
                staminaLoss +
                delay * Mathf.Max(0f, telegraphLoss) +
                (canAttackNow
                    ? 0f
                    : Mathf.Max(
                        0f,
                        desperationInstantAttackPreference
                    ));

            totalLoss = Mathf.Lerp(totalLoss, desperateLoss, desperation);
        }

        return totalLoss;
    }

    private void TryAttack(IAttack attack, Vector2 playerPosition)
    {
        if (!IsAttackAvailable(attack) ||
            Time.fixedTime < _nextAttackTime)
        {
            return;
        }

        if (Mathf.Max(0f, attack.GetStaminaCost()) >
                staminaEntity.GetStamina() ||
            !attack.CanHit(playerPosition))
        {
            return;
        }

        _previousAttack = attack;

        attack.Attack(_player);

        float attackLockDuration = Mathf.Max(
            minimumAttackInterval,
            attack.GetDelay()
        );

        _nextAttackTime = Time.fixedTime + Mathf.Max(0.02f, attackLockDuration);

        if (!canRepeatAttacks)
        {
            _nextAttackDecisionTime = Time.fixedTime;
        }
    }

    private void MoveForAttack(IAttack attack, Vector2 playerPosition)
    {
        if (!IsAttackAvailable(attack))
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        float safeRange = Mathf.Max(attack.GetRange(), 0.001f);
        float currentDistance = Vector2.Distance(playerPosition, _rb.position);

        float signedApproachStrength =
            (currentDistance / safeRange - 0.8f) / 0.4f;

        Vector2 toPlayer = playerPosition - _rb.position;
        Vector2 approachDirection = Vector2.zero;

        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            approachDirection =
                toPlayer.normalized * Mathf.Sign(signedApproachStrength);
        }

        float approachMagnitude =
            Mathf.Clamp01(Mathf.Abs(signedApproachStrength));

        if (Time.fixedTime >= _nextCrowdCheckTime)
        {
            RefreshDisperseVector();
            _nextCrowdCheckTime =
                Time.fixedTime + Mathf.Max(0.02f, crowdCheckInterval);
        }

        float disperseStrength =
            attack.IsAoe()
                ? aoeDecrowdingStrength
                : constantDecrowdingStrength;
        disperseStrength = Mathf.Max(0f, disperseStrength);
        
        //print("ApproachDirection: " + approachDirection + ", DisperseVector: " + _cachedDisperseVector + ", ApproachStrength: " + approachMagnitude + ", DisperseStrength: " + disperseStrength);

        Vector2 desiredMovement =
            approachDirection * approachMagnitude +
            _cachedDisperseVector * disperseStrength;

        float safeForceDropoff = Mathf.Max(forceDropoff, 0.001f);
        float movementSpeed = Mathf.Max(0f, speed);
        Vector2 velocity;

        if (desiredMovement.magnitude >= safeForceDropoff)
        {
            velocity = desiredMovement.normalized * movementSpeed;
        }
        else
        {
            velocity =
                desiredMovement * movementSpeed / safeForceDropoff;
        }

        _rb.linearVelocity = velocity;
    }

    private void RefreshDisperseVector()
    {
        Vector2 separation = Vector2.zero;
        float safeDispersionRadius = Mathf.Max(0f, dispersionRadius);
        float dispersionRadiusSquared =
            safeDispersionRadius * safeDispersionRadius;

        if (GnomeTracker.Instance == null)
        {
            _cachedDisperseVector = Vector2.zero;
            return;
        }

        foreach (GnomeAI gnomeAI in GnomeTracker.Instance.GetGnomeEnumerator())
        {
            if (gnomeAI == null ||
                gnomeAI.gameObject.Equals(this.gameObject))
            {
                continue;
            }

            if (Vector2.SqrMagnitude(
                    gnomeAI.transform.position - transform.position
                ) >= dispersionRadiusSquared)
            {
                continue;
            }
            GameObject hit = gnomeAI.gameObject;

            if (!hit || hit == gameObject)
            {
                continue;
            }

            Vector2 awayFromGnome =
                _rb.position - (Vector2)gnomeAI.transform.position;
            
            float awayFromGnomeSqrMag = awayFromGnome.sqrMagnitude;
            
            //print("Away: " + awayFromGnome + ", " + awayFromGnomeSqrMag);

            if (awayFromGnomeSqrMag > 0.0001f)
            {
                separation += awayFromGnome / awayFromGnomeSqrMag;
            }
        }

        _cachedDisperseVector =
            separation.sqrMagnitude > 0.0001f
                ? separation.normalized
                : Vector2.zero;
        
        //print("Refreshed: " + separation + " to " + _cachedDisperseVector);
    }

    private void UpdateAnimation()
    {
        if (!_animator || !_rb)
        {
            return;
        }

        _animator.speed = Mathf.Max(0f, animationSpeed);

        Vector2 velocity = _rb.linearVelocity;
        float movementThreshold =
            Mathf.Max(0f, animationMovementThreshold);

        if (velocity.sqrMagnitude <
            movementThreshold * movementThreshold)
        {
            return;
        }

        float absX = Mathf.Abs(velocity.x);
        float absY = Mathf.Abs(velocity.y);
        float bias = Mathf.Max(0f, directionHysteresis);

        FacingDirection newDirection = _facingDirection;

        // Dont spam random directions
        if (absX > absY + bias)
        {
            newDirection =
                velocity.x >= 0f
                    ? FacingDirection.Right
                    : FacingDirection.Left;
        }
        else if (absY > absX + bias)
        {
            newDirection =
                velocity.y >= 0f
                    ? FacingDirection.Up
                    : FacingDirection.Down;
        }

        if (newDirection != _facingDirection)
        {
            _facingDirection = newDirection;
            ApplyFacingDirection();
        }
    }

    private void ApplyFacingDirection()
    {
        if (!_animator ||
            _appliedFacingDirection == _facingDirection)
        {
            return;
        }

        // Every direction is assigned every time, so only one can remain true.
        _animator.SetBool(Up, _facingDirection == FacingDirection.Up);
        _animator.SetBool(Down, _facingDirection == FacingDirection.Down);
        _animator.SetBool(Left, _facingDirection == FacingDirection.Left);
        _animator.SetBool(Right, _facingDirection == FacingDirection.Right);

        _appliedFacingDirection = _facingDirection;
    }

    private void OnDestroy()
    {
        if (GnomeTracker.Instance != null)
        {
            GnomeTracker.Instance.RemoveGnome(this);
        }
    }
}
