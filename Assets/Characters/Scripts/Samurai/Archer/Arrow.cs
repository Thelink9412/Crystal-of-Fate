using UnityEngine;

//Script che gestisce la freccia del Samurai Archer
public class Arrow : MonoBehaviour
{
    [SerializeField] private float speed = 10f;

    private float damage;
    private Transform target;
    private EnemyBase actualTarget;

    public void Init(Vector3 targetPosition, float d, SamuraiArcher archer)
    {
        PlayerBase player = archer.GetComponent<PlayerBase>();
        if (player != null && player.currentEnemy != null)
        {
            target = player.currentEnemy.transform;
            actualTarget = player.currentEnemy;
            damage = d;
        }
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (target == null)
        {
            Debug.Log("Target Null!");
            Destroy(gameObject);
            return;
        }
        if (GameManager.Instance.IsPaused) return;
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy == null || enemy != actualTarget)
            return;

        speed = 0;
        enemy.TakeDamage(damage);
        Destroy(gameObject);
    }
}
