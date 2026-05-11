//State Tracking Enums
public enum EnemyState
{
    Idle,      // not moving or attacking
    Walking,   // moving to a static target (tower) or patrol point
    Chasing,   // chasing a moving target (player)
    Attacking,  // attacking current target (tower/player)
    Hit,    // Enemy is staggered, short term stutter then transition back to other state
    Dead,       // Enemy is dead, stop all activity
    Returning   // Enemy is returning to their spawn point
}