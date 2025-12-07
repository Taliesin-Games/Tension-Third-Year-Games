using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(EnemyController))]
public class Enemy : Character
{
    private static int count;
    public static int EnemyCount => count;

    public static void Increment() => count++;
    public static void Decrement() => Mathf.Min(0, count--);

    bool isDead;

    public bool IsDead { get { return isDead; } set { isDead = value; } }

    private void OnDestroy()
    {
        EnemySpawner.Instance.RemoveEnemy(gameObject);
        GameManager.Instance.OnEnemyDefeated(); // TODO need to move this to object pooler, die method or on disable with a flag to prevent multiple calls
    }
}
