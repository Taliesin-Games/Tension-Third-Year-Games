using System;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

[Obsolete("Use EnemyController instead")]
public class EnemyAI : MonoBehaviour
{
    #region Confguration
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
    EnemyState currentState = EnemyState.Idle;
    TargetKind currentTargetKind = TargetKind.None;

    float chaseRepathTimer = 0f;
    float nextAttackTime = 0f;

    // Patrol state
    Vector3 patrolOrigin;
    Vector3 patrolDestination;
   float patrolWaitTimer = 0f;

    // Detection runtime
    float loseTargetTimer = 0f;
    #endregion

    #region Properties
    bool IsDead => currentState == EnemyState.Dead;
    #endregion

    void Awake()
    {
        // Cache references
        enemy = GetComponent<Enemy>();
        enemyNavigation = GetComponent<EnemyNavigation>();

        //Set initial origin for patrols
        patrolOrigin = transform.position; // patrol around spawn
    }
    void Update()
    {
        
        if (IsDead) return; // Dead enemies do nothing

        // Temporary: die on P key for testing
        if (Input.GetKeyDown(KeyCode.P))
        {
            Die();
        }

        // State machine tick
        switch (currentState)
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
            case EnemyState.Hit:
                TickHit();
                break;

            case EnemyState.Returning:
                TickReturning();
                break;
        }

        // Debug drawing
        if (drawDebug)
        {
            Helpers.DebugDrawCircle(patrolOrigin, patrolRadius, Color.cyan); // Patrol area
            Helpers.DebugDrawCircle(transform.position + Vector3.up * 0.05f, detectionRadius, Color.yellow); // Detection radius
        }
    }
    private void TickReturning()
    {
        throw new NotImplementedException();
    }
    private void TickHit()
    {
        throw new NotImplementedException();
    }
    void TickIdle()
    {
        // First, try to detect something to attack
        if (TryDetectAndSetTarget())
            return;

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
                currentTarget = null;
                currentTargetKind = TargetKind.None;
                currentState = EnemyState.Walking;
                return;
            }
        }

        // No valid point this frame; try shortly again
        patrolWaitTimer = patrolWaitTimeMax; 
        patrolOrigin = transform.position;
    }
    void TickWalking()
    {
        // If walking without a target we are patrolling
        if (currentTarget == null)
        {
            if (enemyNavigation.HasReachedDestination())
            {
                patrolWaitTimer = Random.Range(patrolPauseRange.x, patrolPauseRange.y);
                currentState = EnemyState.Idle;
            }

            // while patrolling still scan for enemies
            TryDetectAndSetTarget();

            return;
        }

        // Walking toward a target, If we reached the target, start attacking
        if (IsWithinAttackRange(currentTarget.position))
        {
            currentState = EnemyState.Attacking;
            return;
        }

        // if target disappeared
        if (currentTarget == null)
        {
            ResetTarget();
            return;
        }
    }
    void TickChasing()
    {
        // If target vanished (destroyed), reset state machine
        if (currentTarget == null)
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
            currentState = EnemyState.Attacking;
            return;
        }
    }
    void TickAttacking()
    {
        // If target vanished (destroyed), reset and try again.
        if (currentTarget == null)
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
                currentState = EnemyState.Chasing;
                return;
            }
            else
            {
                // For static target, walk back into range
                if (enemyNavigation.MoveTo(currentTarget.position))
                {
                    currentState = EnemyState.Walking;
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
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            enemy.EnemyAIAttack();
            
        }

        // Debug drawing for attacks
        if (drawDebug)
        {
            Helpers.DebugDrawCircle(transform.position, attackRange, Color.red);
        }
    }
    void BeginPursuit(Transform target, TargetKind kind, EnemyState state)
    {
        currentTarget = target;
        currentTargetKind = kind;
        currentState = state;
        chaseRepathTimer = 0f;
        if (state == EnemyState.Chasing)
        {
            loseTargetTimer = loseTargetAfter;
        }
    }
    void ResetTarget()
    {
        // Clear target and return to idle, effectively restarting the state machine
        currentTarget = null;
        currentTargetKind = TargetKind.None;
        currentState = EnemyState.Idle;
        loseTargetTimer = 0f;
    }
    bool IsWithinAttackRange(Vector3 targetPos)
    {
        return (targetPos - transform.position).sqrMagnitude <= (attackRange * attackRange);
    }

    // Try to detect a Player or Attackable in radius; if found, switch state
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
            var h = hits[i];
            if (h == null) continue;
            var tr = h.transform;

            // Prefer root or rigidbody transform
            var rb = tr.GetComponent<Rigidbody>();
            if (rb != null) tr = rb.transform;

            // Identify candidate by tag and components
            TargetKind k = TargetKind.None;
            if (tr.CompareTag("Player") || tr.GetComponentInParent<CharacterController>() != null)
                k = TargetKind.Player;
            else if (tr.CompareTag("Attackable"))
                k = TargetKind.Tower;
            else
                continue; // not a target

            // Target center
            Vector3 tgt = tr.position + Vector3.up * eyeHeight;
            Vector3 dir = tgt - eyes;
            float distSqr = dir.sqrMagnitude;
            if (distSqr > detectionRadius * detectionRadius) continue;

            // FOV check
            if (detectionFOVDegrees < 359f)
            {
                float ang = Vector3.Angle(transform.forward, dir);
                if (ang > detectionFOVDegrees * 0.5f) continue;
            }

            // LOS check
            if (losObstructionMask.value != 0)
            {
                float dist = Mathf.Sqrt(distSqr);
                if (Physics.Raycast(eyes, dir.normalized, dist, losObstructionMask, QueryTriggerInteraction.Ignore))
                {
                    continue; // blocked
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

        // Return best found target
        if (best != null)
        {
            target = best;
            kind = bestKind;
            return true;
        }

        return false;
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
            if (ang > detectionFOVDegrees * 0.5f)
                return false;
        }

        // LOS
        if (losObstructionMask.value != 0)
        {
            float dist = dir.magnitude;
            if (Physics.Raycast(eyes, dir.normalized, dist, losObstructionMask, QueryTriggerInteraction.Ignore))
                return false;
        }

        return true;
    }
    public void Die()
    {
        if (IsDead) return;
        currentState = EnemyState.Dead;

        // enemyNavigation.Die(); removed due to switching to properties
        Debug.Log($"{gameObject.name} (Enemy) is handling death logic.");
        Enemy.Decrement();

        enemy.EnemyAIDie();

        Destroy(gameObject, 1f);
    }
    private void OnDestroy()
    {
        //GameManager.Instance.CheckWinGame();
    }
    // Apply damage to current target if cooldown elapsed
    public void TryDealDamageToCurrentTarget()
    {

        var targetGo = currentTarget != null ? currentTarget.gameObject : null;
        if (targetGo == null) return;

        //REMOVE COMMENTS WHEN HEALTH SCRIPT IS READY
        /*
        var health = targetGo.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(attackDamage);
        }
        */
    }


}
