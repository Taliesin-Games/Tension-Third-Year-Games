using BMD.DataTypes;
using BMD.ProcGen;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
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
    [SerializeField] FloatRange meleeAttackRange = new FloatRange(0.5f, 1.5f);
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
    [SerializeField] bool isPatrolling = false;
    [SerializeField] float patrolWaitTimer = 0f;

    // Detection runtime
    float loseTargetTimer = 0f;
    #endregion

    #region Preallocations
    Vector2 inputDirectiuon = Vector2.zero;
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

        enemyState = EnemyState.Chasing;
        currentTarget = Player.Instance.transform;
        loseTargetTimer = Time.time;
    }
    private void DefineAttackChoices()
    {
        // Predefine the dictionary of attack choices
        attackChoices[Chase] = 0;
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
        else                                   enemyState = EnemyState.Attacking;
        
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
                Chase();
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
            moveDirection = Vector3.zero;
        }
        return result;
    }
    void WalkHome()
    {
        if (HasReachedDestination()) moveDirection = DirectionToHome;
        else
        {
            inputDirectiuon = enemyNavigation.MoveDirection();
            moveDirection.x = inputDirectiuon.x;
            moveDirection.z = inputDirectiuon.y;
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
            inputDirectiuon = enemyNavigation.MoveDirection();
            moveDirection.x = inputDirectiuon.x;
            moveDirection.z = inputDirectiuon.y;
            moveDirection.y = 0f;
        }

        // Walk back slightly slower
        moveDirection *= 0.8f;

        SoftStop(ref moveDirection, DistanceToPatrolPoint);
    }
    void Chase() 
    {
        enemyNavigation.MoveTo(Player.Instance.transform.position); // Update player position every frame while chasing.
        inputDirectiuon = enemyNavigation.MoveDirection();
        moveDirection.x = inputDirectiuon.x;
        moveDirection.z = inputDirectiuon.y;
        moveDirection.y = 0f;

        SoftStop(ref moveDirection, DistanceToTarget);

        if (DistanceToTarget >= detectionRadius && Time.time > loseTargetTimer)
        {
            currentTarget = null;
            enemyState = EnemyState.Idle;
            enemyNavigation.MoveTo(homePosition);
        }
        else
        {
            loseTargetTimer = Time.time;
            if (DistanceToTarget < MaxAttackRange) enemyState = EnemyState.Attacking;
        }
    }
    void Attack()
    {
        // If enemy has moved away, just chase to move closer. We don't switch state for a more responsive attack.
        if (DistanceToTarget > MaxAttackRange)
        {
            Chase();
            return;
        }
        enemyNavigation.MoveTo(Player.Instance.transform.position);

        // Reset the scores
        foreach(var entry in attackChoices)
        {
            attackChoices[entry.Key] = 0;
        }



    }
    void AttackMelee()
    {

    }
    void AttackRanged()
    {

    }
    void AttackSpell()
    {

    }
    void ChooseWeighted(List<(System.Action action, int weight)> options)
    {
        int total = 0;

        foreach (var option in options)
            total += option.weight;

        int roll = Random.Range(0, total);

        int current = 0;

        foreach (var option in options)
        {
            current += option.weight;

            if (roll < current)
            {
                option.action.Invoke();
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
    private void SetMoveDirection()
    {
        Vector2 inputDirectiuon = enemyNavigation.MoveDirection(); // a Vector3 direction
        Vector3 worldDirection = new Vector3(inputDirectiuon.x,0, inputDirectiuon.y);

        float inputMagnitude = Mathf.Clamp01(worldDirection.magnitude);
        inputMagnitude = Mathf.Pow(inputMagnitude, 1.5f);

        moveDirection = worldDirection.normalized * inputMagnitude;
        moveDirection = worldDirection.normalized * inputMagnitude;
    }
    private void SwitchEnemyState()
    {
        // State machine tick
        switch (enemyState)
        {
            case EnemyState.Idle:
                TickIdle(); // Intermediary state, tries to find targets or start patrols
                break;

            case EnemyState.Walking:
                TickWalking(); // Moving to patrol point or static target
                break;

            case EnemyState.Chasing:
                TickChasing(); // Pursuing a moving target
                break;

            case EnemyState.Attacking:
                TickAttacking(); // In attack range of target
                break;
        }
    }
    void TickAttacking()
    {
        // If target vanished (destroyed), reset and try again.
        if (NoTarget)
        {
            ResetTarget();
            return;
        }

        // Face target (optional)
        Vector3 toTarget = currentTarget.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toTarget), 10f * Time.deltaTime);
        }

        // Ensure we are in range; if not, resume appropriate movement.
        if (!IsWithinAttackRange)
        {
            if (currentTargetKind == TargetKind.Player)
            {
                enemyState = EnemyState.Chasing;
                return;
            }
            else
            {
                // For static target, walk back into range
                if (enemyNavigation.MoveTo(currentTarget.position))
                {
                    enemyState = EnemyState.Walking;
                    return;
                }
                else
                {
                    // If we can no longer path to the static target, reset and try other options
                    ResetTarget();
                    return;
                }
            }
        }

        // In range and have a target: attack (actual damage application is driven by animation events)
        if (Time.time > nextAttackTime)
        {
            nextAttackTime = Time.time + meleeAttackCooldown;
            RequestAttack();
        }

        // Debug drawing for attacks
        if (drawDebug) Helpers.DebugDrawCircle(transform.position, meleeAttackRange.Max, Color.red);
    }
    void TickChasing()
    {
        // If target vanished (destroyed), reset state machine
        if (NoTarget)
        {
            ResetTarget();
            return;
        }

        // Maintain pursuit path
        chaseRepathTimer -= Time.deltaTime;
        if (chaseRepathTimer <= 0f)
        {
            enemyNavigation.MoveTo(currentTarget.position);
            chaseRepathTimer = chaseRepathInterval;
        }

        // Visibility check
        if (HasSightOn(currentTarget))
        {
            loseTargetTimer = loseTargetAfter;
        }
        else
        {
            loseTargetTimer -= Time.deltaTime;
            if (loseTargetTimer <= 0f)
            {
                ResetTarget();
                return;
            }
        }

        // If within attack range, start attacking
        if (IsWithinAttackRange)
        {
            enemyState = EnemyState.Attacking;
            return;
        }
    }
    void TickWalking()
    {
        // While patrolling, keep scanning for targets
        if (isPatrolling)
        {
            if (TryDetectAndSetTarget()) return;

            if (enemyNavigation.HasReachedDestination())
            {
                isPatrolling = false;
                patrolWaitTimer = Random.Range(patrolPauseRange.x, patrolPauseRange.y);
                enemyState = EnemyState.Idle;
                return;
            }
            return;
        }

        // Walking towards a non-patrol target (e.g., tower)
        if (NoTarget)
        {
            ResetTarget();
            return;
        }

        // If we reached the target, start attacking
        if (IsWithinAttackRange)
        {
            enemyState = EnemyState.Attacking;
            return;
        }
    }
    void TickIdle()
    {
        // First, try to detect something to attack
        if (TryDetectAndSetTarget()) return;

        // Patrol: wait, then choose a new patrol point via navigation helper
        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -= Time.deltaTime;
            return;
        }

        // Try to get a patrol point
        if (enemyNavigation.TryGetPatrolPoint(homePosition, patrolRadius, navMeshSampleRadius, patrolSampleMaxTries, out patrolDestination))
        {
            // Start moving to it if possible
            if (enemyNavigation.MoveTo(patrolDestination))
            {
                isPatrolling = true;
                currentTarget = null;
                currentTargetKind = TargetKind.None;
                enemyState = EnemyState.Walking;
                return;
            }
        }

        // No valid point this frame; try shortly again
        patrolWaitTimer = patrolWaitTimeMax;
        homePosition = transform.position;
    }
    void ResetTarget()
    {
        // Clear target and return to idle, effectively restarting the state machine
        currentTarget = null;
        currentTargetKind = TargetKind.None;
        isPatrolling = false;
        enemyState = EnemyState.Idle;
        loseTargetTimer = 0f;
    }
    bool TryDetectAndSetTarget()
    {
        if (TryAcquireTarget(out var t, out var kind))
        {
            if (kind == TargetKind.Player)
            {
                BeginPursuit(t, kind, EnemyState.Chasing);
            }
            else // Tower/static
            {
                if (enemyNavigation.MoveTo(t.position))
                {
                    BeginPursuit(t, kind, EnemyState.Walking);
                }
            }
            return true;
        }
        return false;
    }
    void BeginPursuit(Transform target, TargetKind kind, EnemyState state)
    {
        currentTarget = target;
        currentTargetKind = kind;
        enemyState = state;
        chaseRepathTimer = 0f;
        isPatrolling = false;
        if (state == EnemyState.Chasing)
        {
            loseTargetTimer = loseTargetAfter;
        }
    }
    // Scan nearby colliders to find the best target within FOV/LOS
    bool TryAcquireTarget(out Transform target, out TargetKind kind)
    {
        target = null;
        kind = TargetKind.None;

        int mask = detectionLayerMask.value == 0 ? ~0 : detectionLayerMask.value; // all layers if none specified
        var hits = Physics.OverlapSphere(transform.position, detectionRadius, mask, QueryTriggerInteraction.Ignore); // get all colliders in radius
        if (hits == null || hits.Length == 0) return false; // nothing found, return

        float bestDistSqr = float.MaxValue;
        Transform best = null;
        TargetKind bestKind = TargetKind.None;

        Vector3 eyes = transform.position + Vector3.up * eyeHeight;

        for (int i = 0; i < hits.Length; i++)
        {
            TryProcessHit(
                hits[i], 
                eyes, 
                ref bestDistSqr, 
                ref best, 
                ref bestKind
                );
        }

        // Return best found target
        if (best != null)
        {
            target = best;
            kind = bestKind;
            return true;
        }

        return false;
    }
    private void TryProcessHit(Collider h, Vector3 eyes, ref float bestDistSqr, ref Transform best, ref TargetKind bestKind)
    {
        if (h == null) return;
        var tr = h.transform;

        if (tr == transform) return;        // Exclude pathing to self

        // Prefer root or rigidbody transform
        var rb = tr.GetComponent<Rigidbody>();
        if (rb != null) tr = rb.transform;

        // Identify candidate by tag and components
        TargetKind k = TargetKind.None;
        if (tr.CompareTag("Player") || tr.GetComponentInParent<BMD.PlayerController>() != null) // TODO, getcomponent is heavy, look into throttling
            k = TargetKind.Player;
        else if (tr.CompareTag("Attackable"))
            k = TargetKind.Tower;
        else
            return; // not a target

        // Target center
        Vector3 tgt = tr.position + Vector3.up * eyeHeight;
        Vector3 dir = tgt - eyes;
        float distSqr = dir.sqrMagnitude;
        if (distSqr > detectionRadius * detectionRadius) return;

        // FOV check
        if (detectionFOVDegrees < 359f)
        {
            float ang = Vector3.Angle(transform.forward, dir);
            if (ang > detectionFOVDegrees * 0.5f) return;
        }
        
        // LOS check
        if (losObstructionMask.value != 0)
        {
            float dist = Mathf.Sqrt(distSqr);
            if (Physics.Raycast(eyes, dir.normalized, dist, losObstructionMask, QueryTriggerInteraction.Ignore))
            {
                return; // blocked
            }
        }

        // Better than previous best?
        if (distSqr < bestDistSqr)
        {
            bestDistSqr = distSqr;
            best = tr;
            bestKind = k;
        }
    }
    private void DrawDebug()
    {
        // Debug drawing
        if (!drawDebug) return;

        Helpers.DebugDrawSphere(homePosition, 2, Color.green, 24);          // Home Destination
        Helpers.DebugDrawCircle(homePosition, patrolRadius, Color.cyan);    // Patrol area
        Helpers.DebugDrawSphere(patrolDestination, 2, Color.blue, 24);      // Patrol Destination
        Helpers.DebugDrawCircle(transform.position + Vector3.up * 0.05f, detectionRadius, Color.yellow); // Detection radius
        
    }
    public void Die()
    {
        // TODO die shoudl be handled on character, not character controller.
        // Character controllers only consideration is if it needs to stop or interrupt processes
        if (IsDead) return;

        IsDead = true;

        Enemy.Decrement();

        RequestDie();               
        
    }
    // Check current target visibility (for chase persistence)
    bool HasSightOn(Transform t)
    {
        if (t == null) return false;

        Vector3 eyes = transform.position + Vector3.up * eyeHeight;
        Vector3 tgt = t.position + Vector3.up * eyeHeight;
        Vector3 dir = tgt - eyes;

        // Distance
        if (dir.sqrMagnitude > detectionRadius * detectionRadius)
            return false;

        // FOV
        if (detectionFOVDegrees < 359f)
        {
            float ang = Vector3.Angle(transform.forward, dir);
            if (ang > detectionFOVDegrees * 0.5f) return false;
        }

        // LOS
        if (losObstructionMask.value != 0)
        {
            float dist = dir.magnitude;
            if (Physics.Raycast(eyes, dir.normalized, dist, losObstructionMask, QueryTriggerInteraction.Ignore)) return false;
        }

        return true;
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
