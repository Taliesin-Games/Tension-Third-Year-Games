using UnityEngine;
using BMD;
using Utils;
using Random = UnityEngine.Random;

[RequireComponent(typeof(EnemyNavigation))]
public class EnemyController : BMD.CharacterController
{
    #region Confguration
    [Header("Enemy Configuration")]
    [SerializeField] float attackRange = 1.75f;            // how close we need to be to start attacking
    [SerializeField] float chaseRepathInterval = 0.2f;      // how often to re-issue paths while chasing
    [SerializeField] bool drawDebug = false;

    [Header("Combat")]
    [SerializeField] int attackDamage = 10;                 // damage per hit
    [SerializeField] float attackCooldown = 2.5f;          // attack cadence

    [Header("Patrol Config")] // Patrol config (AI decides when to patrol; navigation provides points)
    [SerializeField] float patrolRadius = 6f;
    [SerializeField] Vector2 patrolPauseRange = new Vector2(0.5f, 1.5f);
    [SerializeField] float patrolSampleMaxDistance = 2f;
    [SerializeField] int patrolSampleMaxTries = 6;

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
    #endregion

    #region Runtime Variables
    Transform currentTarget;
    [Header("Serialised for debugging")]
    [SerializeField] EnemyState enemyState = EnemyState.Idle;
    [SerializeField] TargetKind currentTargetKind = TargetKind.None;

    float chaseRepathTimer = 0f;
    float nextAttackTime = 0f;

    // Patrol state
    Vector3 patrolOrigin;
    Vector3 patrolDestination;
    [SerializeField] bool isPatrolling = false;
    [SerializeField] float patrolWaitTimer = 0f;

    // Detection runtime
    float loseTargetTimer = 0f;
    #endregion

    #region Properties and Helpers
    bool IsDead {  get { return enemy.IsDead; } set { enemy.IsDead = value; } }
    bool NoTarget => currentTarget == null;
    bool IsWithinAttackRange(Vector3 targetPos) => (targetPos - transform.position).sqrMagnitude <= (attackRange * attackRange);
    
    #endregion


    protected override void Awake()
    {
        base.Awake();
        FindReferences();

        //Set initial origin for patrols
        patrolOrigin = transform.position; // patrol around spawn
    }
    private void FindReferences()
    {
        // Cache references
        enemy = GetComponent<Enemy>();
        enemyNavigation = GetComponent<EnemyNavigation>();
    }
    protected override void Update()
    {
        if (IsDead) return;  // Dead enemies do nothing

        SwitchEnemyState();

        DrawDebug();

        moveDirection = enemyNavigation.MoveDirection();

        base.Update();
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
        if (!IsWithinAttackRange(currentTarget.position))
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
            nextAttackTime = Time.time + attackCooldown;
            RequestAttack();
        }

        // Debug drawing for attacks
        if (drawDebug) Helpers.DebugDrawCircle(transform.position, attackRange, Color.red);
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
        if (IsWithinAttackRange(currentTarget.position))
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
        if (IsWithinAttackRange(currentTarget.position))
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
        if (enemyNavigation.TryGetPatrolPoint(patrolOrigin, patrolRadius, patrolSampleMaxDistance, patrolSampleMaxTries, out patrolDestination))
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
        patrolOrigin = transform.position;
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

        // Prefer root or rigidbody transform
        var rb = tr.GetComponent<Rigidbody>();
        if (rb != null) tr = rb.transform;

        // Identify candidate by tag and components
        TargetKind k = TargetKind.None;
        if (tr.CompareTag("Player") || tr.GetComponentInParent<UnityEngine.CharacterController>() != null) // TODO, getcomponent is heavy, look into throttling
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
        
        Helpers.DebugDrawCircle(patrolOrigin, patrolRadius, Color.cyan); // Patrol area
        Helpers.DebugDrawCircle(transform.position + Vector3.up * 0.05f, detectionRadius, Color.yellow); // Detection radius
        
    }
    public void Die()
    {
        // TODO die shoudl be handled on character, not character controller.
        // Character controllers only consideration is if it needs to stop or interrupt processes
        if (IsDead) return;

        IsDead = true;

        Debug.Log($"{gameObject.name} (Enemy) is handling death logic.");
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
}
