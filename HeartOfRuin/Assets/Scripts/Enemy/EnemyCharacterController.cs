using UnityEngine;
using BMD;

[RequireComponent(typeof(EnemyNavigation))]
public class EnemyCharacterController : BMD.CharacterController
{
    #region Confguration
    [SerializeField] float attackRange = 1.75f;            // how close we need to be to start attacking
    [SerializeField] float chaseRepathInterval = 0.2f;      // how often to re-issue paths while chasing
    [SerializeField] bool drawDebug = false;
    [SerializeField] float defaultDeathDelay = 0.1f;

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
    [SerializeField] EnemyState currentState = EnemyState.Idle;
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

    #region Properties
    bool IsDead {  get { return enemy.IsDead; } set { enemy.IsDead = value; } }
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

        // Temporary: die on P key for testing
        if (Input.GetKeyDown(KeyCode.P))
        {
            Die();
        }

        SwitchEnemyState();
        DrawDebug();

        base.Update();
    }

    private void SwitchEnemyState()
    {
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
        }
    }
    private void DrawDebug()
    {
        // Debug drawing
        if (drawDebug)
        {
            DebugDrawCircle(patrolOrigin, patrolRadius, Color.cyan); // Patrol area
            DebugDrawCircle(transform.position + Vector3.up * 0.05f, detectionRadius, Color.yellow); // Detection radius
        }
    }

    public void Die()
    {
        if (IsDead) return;

        IsDead = true;

        enemyNavigation.Die();

        Debug.Log($"{gameObject.name} (Enemy) is handling death logic.");
        Enemy.Decrement();
        RequestDie();

        Destroy(gameObject, defaultDeathDelay);
    }
}
