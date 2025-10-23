using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{


    static int count = 0;
    public static int EnemyCount => count;

    enum EnemyState
    {
        Idle,      // not moving or attacking
        Walking,   // moving to a static target (tower) or patrol point
        Chasing,   // chasing a moving target (player)
        Attacking  // attacking current target (tower/player)
    }
    enum TargetKind
    {
        None,
        Player,
        Tower
    }

    #region Confguration
    [SerializeField] float attackRange = 1.75f;            // how close we need to be to start attacking
    [SerializeField] float chaseRepathInterval = 0.2f;      // how often to re-issue paths while chasing
    [SerializeField] bool drawDebug = false;

    // Combat
    [SerializeField] int attackDamage = 10;                 // damage per hit
    [SerializeField] float attackCooldown = 2.5f;          // attack cadence

    // Patrol config (AI decides when to patrol; navigation provides points)
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
    #endregion

    #region Cached References
    EnemyNavigation enemyNavigation;
    Animator animator;
    #endregion


    #region Runtime Variables
    Transform currentTarget;
    [SerializeField] EnemyState currentState = EnemyState.Idle;
    [SerializeField] TargetKind currentTargetKind = TargetKind.None;

    float chaseRepathTimer = 0f;
    float nextAttackTime = 0f;

    Boolean isDead = false;

    // Patrol state
    Vector3 patrolOrigin;
    Vector3 patrolDestination;
    [SerializeField] bool isPatrolling = false;
    [SerializeField] float patrolWaitTimer = 0f;

    // Detection runtime
    float loseTargetTimer = 0f;
    #endregion

    /*
    private void OnEnable()
    {
        count++;
        Tower.enemies.Add(this);
    }
    private void OnDisable()
    {
        Tower.enemies.Remove(this);
    }
    */

    void Awake()
    {
        enemyNavigation = GetComponent<EnemyNavigation>();
        animator = GetComponent<Animator>();

        patrolOrigin = transform.position; // patrol around spawn
    }
    void Update()
    {
        if (isDead) return;
        if (Input.GetKeyDown(KeyCode.P))
        {
            Die();
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                TickIdle();
                break;

            case EnemyState.Walking:
                TickWalking();
                break;

            case EnemyState.Chasing:
                TickChasing();
                break;

            case EnemyState.Attacking:
                TickAttacking();
                break;
        }

        if (drawDebug)
        {
            DebugDrawCircle(patrolOrigin, patrolRadius, Color.cyan);
            DebugDrawCircle(transform.position + Vector3.up * 0.05f, detectionRadius, Color.yellow);
        }
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

        if (enemyNavigation.TryGetPatrolPoint(patrolOrigin, patrolRadius, patrolSampleMaxDistance, patrolSampleMaxTries, out patrolDestination))
        {
            if (enemyNavigation.MoveTo(patrolDestination))
            {
                isPatrolling = true;
                currentTarget = null;
                currentTargetKind = TargetKind.None;
                currentState = EnemyState.Walking;
                return;
            }
        }

        // No valid point this frame; try shortly again
        patrolWaitTimer = 0.25f;
        patrolOrigin = transform.position;
    }
    void TickWalking()
    {
        // While patrolling, keep scanning for targets
        if (isPatrolling)
        {
            if (TryDetectAndSetTarget())
                return;

            if (enemyNavigation.HasReachedDestination())
            {
                isPatrolling = false;
                patrolWaitTimer = Random.Range(patrolPauseRange.x, patrolPauseRange.y);
                currentState = EnemyState.Idle;
                return;
            }
            return;
        }

        // Walking towards a non-patrol target (e.g., tower)
        if (currentTarget == null)
        {
            ResetTarget();
            return;
        }

        if (IsWithinAttackRange(currentTarget.position))
        {
            currentState = EnemyState.Attacking;
            return;
        }
    }
    void TickChasing()
    {
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
                // For tower, walk back into range
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
        if (Time.time > nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            animator.SetTrigger("Attack");
        }

        if (drawDebug)
        {
            DebugDrawCircle(transform.position, attackRange, Color.red);
        }
    }
    void BeginPursuit(Transform target, TargetKind kind, EnemyState state)
    {
        currentTarget = target;
        currentTargetKind = kind;
        currentState = state;
        chaseRepathTimer = 0f;
        isPatrolling = false;
        if (state == EnemyState.Chasing)
        {
            loseTargetTimer = loseTargetAfter;
        }
    }
    void ResetTarget()
    {
        currentTarget = null;
        currentTargetKind = TargetKind.None;
        isPatrolling = false;
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

        int mask = detectionLayerMask.value == 0 ? ~0 : detectionLayerMask.value;
        var hits = Physics.OverlapSphere(transform.position, detectionRadius, mask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return false;

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

            // Identify candidate by tag
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

            if (distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                best = tr;
                bestKind = k;
            }
        }

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

    // Overload that searches from a specific origin (kept for future use)
    bool TryFindNearestAttackable(out GameObject target, out TargetKind kind, Vector3 origin)
    {
        target = null;
        kind = TargetKind.None;

        GameObject[] candidates;
        try
        {
            candidates = GameObject.FindGameObjectsWithTag("Attackable");
        }
        catch
        {
            return false;
        }

        float minDistSqr = float.MaxValue;
        foreach (var obj in candidates)
        {
            if (obj == null || obj == gameObject) continue;

            float distSqr = (obj.transform.position - origin).sqrMagnitude;
            if (distSqr < minDistSqr)
            {
                minDistSqr = distSqr;
                target = obj;
            }
        }

        if (target == null) return false;

        var likelyAgent = target.GetComponentInParent<CharacterController>();
        if (likelyAgent != null && likelyAgent.gameObject != gameObject)
        {
            kind = TargetKind.Player;
        }
        else
        {
            if (target.name.IndexOf("player", StringComparison.OrdinalIgnoreCase) >= 0)
                kind = TargetKind.Player;
            else
                kind = TargetKind.Tower;
        }
        return true;
    }

    // Default origin = enemy position
    bool TryFindNearestAttackable(out GameObject target, out TargetKind kind)
    {
        return TryFindNearestAttackable(out target, out kind, transform.position);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        enemyNavigation.Die();
        Debug.Log($"{gameObject.name} (Enemy) is handling death logic.");
        count--;
        if (animator != null)
        {
            animator.SetFloat("DeathType", Random.Range(0, 1));
            animator.SetTrigger("Died");
        }
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

    // Small helper to visualize ranges when debugging
    void DebugDrawCircle(Vector3 center, float radius, Color color, int segments = 24)
    {
        if (!drawDebug) return;
        Vector3 prev = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float ang = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 next = center + new Vector3(Mathf.Cos(ang) * radius, 0, Mathf.Sin(ang) * radius);
            Debug.DrawLine(prev, next, color);
            prev = next;
        }
    }

}
