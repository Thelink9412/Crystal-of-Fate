using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Enemies/EnemyStats")]
public class EnemyStats : ScriptableObject
{
    public float health;
    public float speed;
    public float damage;
    public string enemyName;
}