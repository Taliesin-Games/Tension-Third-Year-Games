using BMD.DataTypes;
using BMD.ProcGen;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Utils;
using Random = UnityEngine.Random;

[RequireComponent(typeof(EnemyNavigation))]
public class EnemyController : BMD.CharacterController
{
    #region Confguration
    [Header("Enemy Configuration")]
    [SerializeField] float stateUpdateInterval = 0.25f;     // how often to update the state machine (can be lower than frame rate for performance)           
    [SerializeField] float chaseRepathInterval = 0.2f;      // how often to re-issue paths while chasing
    [SerializeField] bool drawDebug = false;
    [Tooltip("The range the enemy will start to decelerate.")]
    [SerializeField] float softStopRange = 1.0f;

    [Header("Combat")]
    [SerializeField] float meleeAttackCooldown = 2.5f;          // attack cadence
    [SerializeField] bool meleeAttacker = true;
    [SerializeField] FloatRange meleeAttackRange = new FloatRange(0.0f, 1.5f);
    [SerializeField] float rangedAttackCooldown = 2.5f;
    [SerializeField] bool rangedAttacker = false;
    [SerializeField] FloatRange rangedAttackRange = new FloatRange(3f, 7f);
    [SerializeField] float spellAttackCooldown = 3f;
    [SerializeField] bool spellAttacker = true;
    [SerializeField] FloatRange spellAttackRange = new FloatRange(4f, 8f);

    [Header("Patrol Config")] // Patrol config (AI decides when to patrol; navigation provides points)
    [SerializeField] float patrolRadius = 6f;
    [SerializeField] Vector2 patrolPauseRange = new Vector2(0.5f, 1.5f);
    [SerializeField] float navMeshSampleRadius = 2f;
    [SerializeField] int patrolSampleMaxTries = 6;
    [SerializeField] float patrolIdleTime = 5f; // Time to stay idle before starting to patrol (if no targets found)

    // Detection
    [Header("Detection")]
    [SerializeField] float detectionRadius = 10f;
    [SerializeField] float detectionFOVDegrees = 160f; // 180 = omnidirectional
    [SerializeField] float eyeHeight = 1.6f;
    [SerializeField] LayerMask detectionLayerMask;     // Set to layers containing Player/Attackable. If 0, uses all layers.
    [SerializeField] LayerMask losObstructionMask;     // Set to environment/obstacles that block vision.
    [SerializeField] float loseTargetAfter = 2f;       // seconds to keep chasing after losing sight

    [SerializeField] float patrolWaitTimeMax = 0.25f; // max wait time before trying to find a patrol point, used for failed patrol attempts
    #endregion

    #region Cached References
    Enemy enemy;
    EnemyNavigation enemyNavigation;
    Health health;
    #endregion

    #region Runtime Variables

    Transform currentTarget;
    [Header("Serialised for debugging")]
    [SerializeField] EnemyState enemyState = EnemyState.Idle;
    [SerializeField] TargetKind currentTargetKind = TargetKind.None;
    Coroutine stateUpdateCoroutine;
    Coroutine patrolIdleCoroutine;

    float chaseRepathTimer = 0f;
    float nextAttackTime = 0f;

    // Patrol state
    Vector3 homePosition;
    Vector3 patrolDestination;
    Vector3 fleeDestination;
    [SerializeField] bool isPatrolling = false;
    [SerializeField] float patrolWaitTimer = 0f;

    // Detection runtime
    float loseTargetTimer = 0f;
    #endregion

    #region Preallocations
    Vector2 inputDirection = Vector2.zero;
    Dictionary<System.Action, int> attackChoices = new();
    #endregion
    #region Properties and Helpers
    bool IsDead {  get { return enemy.IsDead; } set { enemy.IsDead = value; } }
    bool NoTarget => currentTarget == null;
    bool IsWithinAttackRange => NoTarget ? false : DistanceToTarget <= MaxAttackRange;
    float DistanceToTarget => NoTarget ? float.MaxValue : FlatDistance(transform.position, currentTarget.position);

    Vector3 DirectionToTarget => NoTarget ? Vector3.zero : FlatDirection(transform.position, currentTarget.position);

    float DistanceToHome => FlatDistance(transform.position, homePosition);

    Vector3 DirectionToHome => FlatDirection(transform.position, homePosition);

    float DistanceToPatrolPoint => FlatDistance(transform.position, patrolDestination);

    Vector3 DirectionToPatrolPoint => FlatDirection(transform.position, patrolDestination);
    bool ReachedPatrolPoint => DistanceToPatrolPoint < softStopRange;

    bool IsInMeleeRange => meleeAttacker ? DistanceToTarget <= meleeAttackRange.Max : false;
    bool IsInRangedAttackRange => rangedAttacker ? DistanceToTarget <= rangedAttackRange.Max : false;
    bool IsInSpellAttackRange => spellAttacker ? DistanceToTarget <= spellAttackRange.Max : false;
    float MinAttackRange => Mathf.Min(meleeAttacker ? meleeAttackRange.Min : float.MaxValue, rangedAttacker ? rangedAttackRange.Min : float.MaxValue, spellAttacker ? spellAttackRange.Min : float.MaxValue);
    float MaxAttackRange => Mathf.Max(meleeAttacker ? meleeAttackRange.Max : float.MinValue, rangedAttacker ? rangedAttackRange.Max : float.MinValue, spellAttacker ? spellAttackRange.Max : float.MinValue);
    float MeanAttackRange => ((meleeAttacker ? meleeAttackRange.Mean : 0) + (rangedAttacker ? rangedAttackRange.Mean : 0) + (spellAttacker ? spellAttackRange.Mean : 0))
                            / ((meleeAttacker ? 1 : 0) + (rangedAttacker ? 1 : 0) + (spellAttacker ? 1 : 0));

    #endregion

    protected override void Awake()
    {
        base.Awake();
        FindReferences();
        DefineAttackChoices();
        //Set initial origin for patrols
        homePosition = transform.position; // patrol around spawn
    }
    private void OnEnable()
    {
        if (health != null) health.OnResourceChanged += TakeDamage;
    }
    private void OnDisable()
    {
        if (health != null) health.OnResourceChanged -= TakeDamage;
    }
    private void TakeDamage(ResourceChangeEventArgs healthData)
    {
        if (healthData.Delta >= 0) return;

        NotifyTakeDamage();

        if (
            enemyState == EnemyState.Idle || 
            enemyState == EnemyState.Patrolling ||
            enemyState == EnemyState.Returning
            )
        {
            StartChase();
            enemyNavigation.MoveTo(currentTarget.position);
        }
    }
    private void DefineAttackChoices()
    {
        // Predefine the dictionary of attack choices
        attackChoices[Chase] = 0;
        attackChoices[Flee] = 0;
        attackChoices[AttackMelee] = 0;
        attackChoices[AttackRanged] = 0;
        attackChoices[AttackSpell] = 0;
    }
    private void FindReferences()
    {
        // Cache references
        enemy = GetComponent<Enemy>();
        enemyNavigation = GetComponent<EnemyNavigation>();
        health = GetComponent<Health>();

    }
    protected override void Start()
    {
        base.Start();
        ResetState(); // Start the state machine loop   
    }
    public void ResetState() 
    {         
        // Stop any ongoing state machine loop and start a new one
        if (stateUpdateCoroutine != null)
        {
            StopCoroutine(stateUpdateCoroutine);
        }
        stateUpdateCoroutine = StartCoroutine(StateUpdateLoop());
    }
    IEnumerator StateUpdateLoop()
    {
        while (!IsDead)
        {
            SetEnemyState();
            yield return new WaitForSeconds(stateUpdateInterval);
        }
    }
    void SetEnemyState()
    {
        switch (enemyState)
        {
            case EnemyState.Idle:
                SetStateFromIdle();
                break;
            case EnemyState.Patrolling:
                SetStateFromPatrolling();
                break;
            case EnemyState.Chasing:
            case EnemyState.Attacking:
                SetStateFromChasing();
                break;
            case EnemyState.Fleeing:
                SetStateFromFleeing();
                break;

        }
    }
    void SetStateFromIdle() 
    {
        if (IsPlayerInRange())
        {
            StartChase();
            if(patrolIdleCoroutine != null)
            {
                StopCoroutine(patrolIdleCoroutine);
                patrolIdleCoroutine = null;
            }
            return;
        }

        // Stay in idle if we are far from home, to avoid weird navigation issues of trying to patrol back to a point we can't reach
        if (DistanceToHome > softStopRange) return;

        // Only restart if not already running, to avoid resetting the timer every frame
        patrolIdleCoroutine ??= StartCoroutine(PatrolIdleTimer());
    }
    IEnumerator PatrolIdleTimer()
    {
        patrolDestination = homePosition;
        yield return new WaitForSeconds(patrolIdleTime);
        SetNewPatrol();
        enemyState = EnemyState.Patrolling;
        patrolIdleCoroutine = null;
    }
    void SetStateFromPatrolling()
    {
        if (IsPlayerInRange())
        {
            StartChase();
            return;
        }
        SetNewPatrol();
    }
    void SetNewPatrol()
    {
        if (DistanceToPatrolPoint > softStopRange) return;

        if (enemyNavigation.TryGetPatrolPoint(homePosition, patrolRadius, navMeshSampleRadius, patrolSampleMaxTries, out patrolDestination))
        {
            enemyNavigation.MoveTo(patrolDestination);
        }
        else
        {
            enemyState = EnemyState.Idle;
            enemyNavigation.MoveTo(homePosition);
        }
    }
    void SetNewFlee() 
    {
        if (enemyNavigation.TryGetPatrolPoint(Player.Instance.transform.position, MaxAttackRange, navMeshSampleRadius, patrolSampleMaxTries, out fleeDestination))
        {
            enemyNavigation.MoveTo(fleeDestination);
        }
        else
        {
            enemyState = EnemyState.Idle;
            enemyNavigation.MoveTo(homePosition);
        }
    }
    void StartChase()
    {
        enemyState = EnemyState.Chasing;
        loseTargetTimer = Time.time;
        currentTarget = Player.Instance.transform;
    }
    void SetStateFromChasing()
    {
        // REturning to idle is handled in the Chase() method, which checks distance and timers every frame, so we only need to check for attack range here.
        if (DistanceToTarget > MaxAttackRange) enemyState = EnemyState.Chasing;     // Cant attack yet, keep chasing
        else enemyState = EnemyState.Attacking;

    }
    void SetStateFromFleeing()
    {
        // REturning to idle is handled in the Chase() method, which checks distance and timers every frame, so we only need to check for attack range here.
        if (DistanceToTarget < MinAttackRange) enemyState = EnemyState.Fleeing;     // Cant attack yet, keep chasing
        else SetStateFromChasing(); // Once we are a safe distance, let Chasing logic decide if we should attack or keep chasing

    }
    bool IsPlayerInRange()
    {
        if (Player.Instance == null) return false;

        Vector3 toPlayer = Player.Instance.transform.position - transform.position;

        // 1. Check range
        if (toPlayer.sqrMagnitude > detectionRadius * detectionRadius) return false;

        // 2. Check line of sight
        Vector3 direction = toPlayer.normalized;
        float distance = toPlayer.magnitude;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance, losObstructionMask))
        {
            // Something blocked the view
            return false;
        }

        loseTargetTimer = Time.time;
        enemyNavigation.MoveTo(Player.Instance.transform.position);
        return true;
    }
    protected override void FixedUpdate()
    {
        if (IsDead) { return; }
        MoveAndAttack();

        base.FixedUpdate();
    }
    private void MoveAndAttack()
    {
        // We use fixed update for enemy inputs. No need to calcualte inputs every frame if they are applied on FixedUpdate.
        switch (enemyState)
        {
            case EnemyState.Idle:
                WalkHome();
                break;
            case EnemyState.Patrolling:
                WalkPatrol();
                break;
            case EnemyState.Chasing:
            case EnemyState.Fleeing:
                ChaseOrFlee();
                break;
            case EnemyState.Attacking:
                Attack();
                break;

        }
    }
    bool HasReachedDestination()
    {
        bool result = enemyNavigation.HasReachedDestination();
        if (result)
        {
            enemyState = EnemyState.Idle;
            enemyNavigation.MoveTo(homePosition);
            moveDirection = Vector3.zero;
        }
        return result;
    }
    void WalkHome()
    {
        if (HasReachedDestination()) moveDirection = DirectionToHome;
        else
        {
            inputDirection = enemyNavigation.MoveDirection();
            moveDirection.x = inputDirection.x;
            moveDirection.z = inputDirection.y;
            moveDirection.y = 0f;
        }

        // Walk back slightly slower
        moveDirection *= 0.8f;

        SoftStop(ref moveDirection, DistanceToHome);

    }
    void WalkPatrol()
    {
        if (HasReachedDestination()) moveDirection = DirectionToPatrolPoint;
        else
        {
            inputDirection = enemyNavigation.MoveDirection();
            moveDirection.x = inputDirection.x;
            moveDirection.z = inputDirection.y;
            moveDirection.y = 0f;
        }

        // Walk back slightly slower
        moveDirection *= 0.8f;

        SoftStop(ref moveDirection, DistanceToPatrolPoint);
    }
    void ChaseOrFlee() 
    {

        if (enemyState == EnemyState.Fleeing) enemyNavigation.MoveTo(fleeDestination);
        else                                  enemyNavigation.MoveTo(Player.Instance.transform.position);
        
         // Update player position every frame while chasing.
        inputDirection = enemyNavigation.MoveDirection();
        moveDirection.x = inputDirection.x;
        moveDirection.z = inputDirection.y;
        moveDirection.y = 0f;

        SoftStop(ref moveDirection, DistanceToTarget);

        if (DistanceToTarget >= detectionRadius && Time.time > loseTargetTimer + loseTargetAfter)
        {
            currentTarget = null;
            enemyState = EnemyState.Idle;
            enemyNavigation.MoveTo(homePosition);
        }
        else
        {
            
            if (DistanceToTarget < MaxAttackRange) enemyState = EnemyState.Attacking;
            if (DistanceToTarget < MinAttackRange && enemyState == EnemyState.Fleeing) enemyState = EnemyState.Fleeing; // Keep fleeing
            if (DistanceToTarget < MinAttackRange && enemyState == EnemyState.Chasing) // Don't update flee destingation every frame, only when switching from chasing.
            {
                enemyState = EnemyState.Fleeing; // Optional: if we get too close, try to back off a bit

                SetNewFlee();
            }
            
        }
    }
    void Attack()
    {
        aimDirection = DirectionToTarget;
        // If enemy has moved away, just chase to move closer. We don't switch state for a more responsive attack.
        if (DistanceToTarget > MaxAttackRange)
        {
            ChaseOrFlee();
            return;
        }
        enemyNavigation.MoveTo(Player.Instance.transform.position);

        // Reset the scores
        attackChoices[Chase] = 0;
        attackChoices[Flee] = 0;
        attackChoices[AttackMelee] = 0;
        attackChoices[AttackRanged] = 0;
        attackChoices[AttackSpell] = 0;

        // Assign some weighting
        attackChoices[Chase] = DistanceToTarget < MinAttackRange ? 0 : 10;  // Never chase when less than min attack range
        attackChoices[Flee] = DistanceToTarget < MeanAttackRange ? 10 : 5;   
        attackChoices[AttackMelee] = IsInMeleeRange ? 30 : 0;
        attackChoices[AttackRanged] = IsInRangedAttackRange ? 30 : 0;
        attackChoices[AttackSpell] = IsInSpellAttackRange ? 30 : 0;

        // Reduce melee attack weight as health gets lower.
        attackChoices[AttackMelee] = (int)Mathf.Ceil(attackChoices[AttackMelee] * health.Normalized);

        ChooseWeightedAttack();
    }
    void Chase()
    {
        enemyState = EnemyState.Chasing;
        ChaseOrFlee();
    }
    void Flee()
    {
        enemyState = EnemyState.Fleeing;
        ChaseOrFlee();
    }
    void AttackMelee()
    {
        if (!meleeAttacker)
        {
            Debug.LogError($"{name} is attempting a melee attack when they are not a melee attacker");
            return;
        }
        RequestAttack();
    }
    void AttackRanged()
    {
        if(!rangedAttacker)
        {
            Debug.LogError($"{name} is attempting a ranged attack when they are not a ranged attacker");
            return;
        }
        RequestFireWeapon();
    }
    void AttackSpell()
    {
        if(!spellAttacker)
        {
            Debug.LogError($"{name} is attempting a spell attack when they are not a spell attacker");
            return;
        }
        RequestSpecialAttack();
    }
    void ChooseWeightedAttack()
    {
        int total = 0;

        foreach (var option in attackChoices)
             total += option.Value;
        
        int roll = Random.Range(0, total);

        
        int current = 0;

        foreach (var option in attackChoices)
        {
            current += option.Value;

            if (roll < current)
            {
                option.Key.Invoke();
                return;
            }
        }
    }
    /// <summary>
    /// When Distance is less than softStopRange scale move direction by remaining distance
    /// </summary>
    /// <param name="moveDirection"></param>
    /// <param name="distance"></param>
    bool SoftStop(ref Vector3 moveDirection, float distance)
    {
        if (distance > softStopRange) return false;
        // Nomaliser remaining distance to soft stop range
        float normalisedDistance = distance / softStopRange;
        moveDirection *= normalisedDistance;

        if (moveDirection.magnitude < 0.01f)
        {
            moveDirection = Vector3.zero;
            
        }

        return true;
    }
    protected override void Update()
    {
        if (IsDead) { base.Update(); return; }  // Dead enemies do nothing


        //SetMoveDirection();

        //SwitchEnemyState();

        DrawDebug();


        base.Update();
    }
    private void DrawDebug()
    {
        // Debug drawing
        if (!drawDebug) return;

        Helpers.DebugDrawSphere(homePosition, 1.5f, Color.green, 24);           // Home Destination
        Helpers.DebugDrawCircle(homePosition, patrolRadius, Color.cyan);        // Patrol area
        Helpers.DebugDrawSphere(patrolDestination, 1.5f, Color.blue, 24);       // Patrol Destination
        Helpers.DebugDrawCircle(transform.position + Vector3.up * 0.05f, detectionRadius, Color.yellow); // Detection radius
        
    }
    public void Die()
    {
        // TODO die should be handled on character, not character controller.
        // Character controllers only consideration is if it needs to stop or interrupt processes
        if (IsDead) return;

        IsDead = true;

        Enemy.Decrement();
        enemyState = EnemyState.Dead;
        inputDirection = Vector2.zero;
        RequestDie();               
        
    }
 
    float FlatDistance(Vector3 a, Vector3 b)
    {
        Vector2 delta = new Vector2(a.x - b.x, a.z - b.z);
        return delta.magnitude;
    }
    Vector3 FlatDirection(Vector3 from, Vector3 to)
    {
        return new Vector3(
            to.x - from.x,
            0f,
            to.z - from.z
        ).normalized;
    }
}
