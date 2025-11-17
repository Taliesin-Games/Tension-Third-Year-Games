using UnityEditor;
using UnityEngine;


[RequireComponent(typeof(EnemyNavigation))]
//[RequireComponent(typeof(Health))]
[RequireComponent(typeof(EnemyAI))]
public class Enemy : MonoBehaviour
{
    void TakeDamage()
    {

    }

    void Attack()
    {

    }
    private void OnDestroy()
    {
        EnemySpawner.Instance.RemoveEnemy(gameObject);
        GameManager.Instance.OnEnemyDefeated(); // TODO need to move this to object pooler, die method or on disable with a flag to prevent multiple calls
    }
}
