using System;
using Entity;
using UnityEngine;

public class GnomeAI : MonoBehaviour
{
    private static readonly int Up = Animator.StringToHash("Up");
    private static readonly int Down = Animator.StringToHash("Down");
    private static readonly int Left = Animator.StringToHash("Left");
    private static readonly int Right = Animator.StringToHash("Right");

    [Header("Movement and Animation")]
    [SerializeField] public float animationSpeed = 1f;
    [SerializeField] public float speed = 2f;
    [SerializeField] public float forceDropoff = 0.2f;
    [SerializeField] private float animationMovementThreshold = 0.05f;
    [SerializeField] private float directionHysteresis = 0.08f;
    [SerializeField] private float dispersionRadius = 5f;

    [Header("AI Update Rates")]
    [Tooltip("How often the gnome re-evaluates every available attack.")]
    [SerializeField] private float decisionInterval = 0.15f;

    [Tooltip("How much better a different attack must be before the gnome switches to it.")]
    [SerializeField] private float attackSwitchThreshold = 0.1f;

    [Tooltip("Minimum time between calls to Attack().")]
    [SerializeField] private float minimumAttackInterval = 0.25f;

    [Tooltip("How often nearby gnomes are checked for crowd separation.")]
    [SerializeField] private float crowdCheckInterval = 0.4f;

    [Header("Attack Preferences")]
    [SerializeField] public bool canRepeatAttacks;

    [SerializeField]
    [Range(-1f, 1f)]
    public float timeConservationPreference = 0f;

    [SerializeField]
    [Range(0f, 1f)]
    public float selfishness = 0.1f;

    [SerializeField] public float outOfRangeLoss = 1f;
    [SerializeField] public float telegraphLoss = 1f;
    [SerializeField] public float desperationTimeCutoff = 5f;
    [SerializeField] public float desperationInstantAttackPreference = 10f;
    [SerializeField] public float constantDecrowdingStrength = 0.1f;
    [SerializeField] public float aoeDecrowdingStrength = 0.5f;

    [Header("Debug")]
    [SerializeField] public float currentLoss;

    public TimeEntity timeEntity;
    public StaminaEntity staminaEntity;

    private GameObject _player;
    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private IAttack[] _attacks;
    private int _group;

    private IAttack _previousAttack;
    private IAttack _chosenAttack;

    private float _nextAttackDecisionTime;
    private float _nextMoveDecisionTime;
    private float _nextAttackTime;
    private float _nextCrowdCheckTime;

    private Vector2 _cachedDisperseVector;
    private FacingDirection _facingDirection = FacingDirection.Down;
    private FacingDirection? _appliedFacingDirection;

    public int GetSortingOrder()
    {
        return _spriteRenderer.sortingOrder;
    }

    private enum FacingDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _player = Player.Player.Instance.gameObject;
        _previousAttack = null;
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _attacks = GetComponents<IAttack>(); // Can no longer edit attack set live in editor
        //_group = GetEntityId().GetHashCode() % GnomeTracker.CycleSize;
        
        staminaEntity.ResetStamina();

        _animator.speed = Mathf.Max(0f, animationSpeed);
        ApplyFacingDirection();

        GnomeTracker.Instance.AddGnome(this);
    }

    private void Update()
    {
        _spriteRenderer.sortingOrder = Mathf.RoundToInt(gameObject.transform.position.y * -100 + 100000);
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

        if (_chosenAttack == null || Time.fixedTime >= _nextAttackDecisionTime)
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

        bool currentAttackWasEvaluated = false;
        float currentAttackLoss = float.PositiveInfinity;

        bool desperate = timeEntity.GetTime() <= desperationTimeCutoff;
        float desperation = timeEntity.GetTime() / desperationTimeCutoff;
        
        foreach (IAttack attack in _attacks)
        {
            if (attack == null)
            {
                continue;
            }

            if (!canRepeatAttacks && attack == _previousAttack)
            {
                continue;
            }

            float loss = EvaluateAttackLoss(attack, playerPosition);

            if (attack == _chosenAttack)
            {
                currentAttackWasEvaluated = true;
                currentAttackLoss = loss;
            }

            if (loss < bestLoss)
            {
                bestAttack = attack;
                bestLoss = loss;
            }
        }

        if (bestAttack == null)
        {
            throw new InvalidOperationException(
                "No attack could be selected. A gnome with only one attack cannot use " +
                "Can Repeat Attacks = false after that attack has been used."
            );
        }

        if (_chosenAttack != null &&
            currentAttackWasEvaluated &&
            currentAttackLoss <= bestLoss + Mathf.Max(0f, attackSwitchThreshold))
        {
            currentLoss = currentAttackLoss;
            return _chosenAttack;
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

        int friendlyFireCount = 0;

        foreach (GameObject hit in attack.GetAllInRange())
        {
            if (!hit || hit == gameObject)
            {
                continue;
            }

            GnomeAI otherGnome =
                GnomeTracker.Instance.GetGnome(hit.GetEntityId());

            if (otherGnome)
            {
                friendlyFireCount++;
            }
        }

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

        if (!attack.InRange(playerPosition))
        {
            totalLoss += outOfRangeLoss;
        }

        totalLoss += delay * telegraphLoss;

        if (desperate)
        {
            bool canAttackNow =
                staminaCost <= staminaEntity.GetStamina() &&
                attack.InRange(playerPosition);

            float desperateLoss =
                staminaLoss +
                delay * telegraphLoss +
                (canAttackNow ? 0f : desperationInstantAttackPreference);

            totalLoss = Mathf.Lerp(totalLoss, desperateLoss, desperation);
        }

        return totalLoss;
    }

    private void TryAttack(IAttack attack, Vector2 playerPosition)
    {
        if (attack == null || Time.fixedTime < _nextAttackTime)
        {
            return;
        }

        if (attack.GetStaminaCost() > staminaEntity.GetStamina() ||
            !attack.InRange(playerPosition))
        {
            return;
        }

        _previousAttack = attack;

        if (attack is IAttackArea areaAttack)
        {
            areaAttack.Attack();
        }
        else if (attack is IAttackTargeted targetedAttack)
        {
            targetedAttack.Attack(_player);
        }
        else
        {
            throw new NotImplementedException(
                $"The attack {attack} implements IAttack but is neither " +
                $"{nameof(IAttackArea)} nor {nameof(IAttackTargeted)}."
            );
        }

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
        if (attack == null)
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
            RefreshDisperseVector(attack);
            _nextCrowdCheckTime =
                Time.fixedTime + Mathf.Max(0.02f, crowdCheckInterval);
        }

        float disperseStrength =
            attack is IAttackArea
                ? aoeDecrowdingStrength
                : constantDecrowdingStrength;
        
        //print("ApproachDirection: " + approachDirection + ", DisperseVector: " + _cachedDisperseVector + ", ApproachStrength: " + approachMagnitude + ", DisperseStrength: " + disperseStrength);

        Vector2 desiredMovement =
            approachDirection * approachMagnitude +
            _cachedDisperseVector * disperseStrength;

        float safeForceDropoff = Mathf.Max(forceDropoff, 0.001f);
        Vector2 velocity;

        if (desiredMovement.magnitude >= safeForceDropoff)
        {
            velocity = desiredMovement.normalized * speed;
        }
        else
        {
            velocity = desiredMovement * speed / safeForceDropoff;
        }

        _rb.linearVelocity = velocity;
    }

    private void RefreshDisperseVector(IAttack attack)
    {
        Vector2 separation = Vector2.zero;

        foreach (GnomeAI gnomeAI in GnomeTracker.Instance.GetGnomeEnumerator())
        {
            if (gnomeAI.gameObject.Equals(this.gameObject)) continue;
            if (Vector2.SqrMagnitude(gnomeAI.gameObject.transform.position - this.gameObject.transform.position) >= dispersionRadius * dispersionRadius) continue;
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