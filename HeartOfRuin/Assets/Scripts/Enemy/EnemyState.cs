//State Tracking Enums
public enum EnemyState
{
    Idle,      // not moving or attacking
    Walking,   // moving to a static target (tower) or patrol point
    Chasing,   // chasing a moving target (player)
    Attacking  // attacking current target (tower/player)
}