using UnityEngine;

//Script che gestisce le spine della stanza
public class Spikes : MonoBehaviour
{
    private Animator animator;
    void Start()
    {

    }

    public System.Collections.IEnumerator ActivateSpikes(float damage)
    {
        animator = GetComponent<Animator>();
        animator.Play("Spikes");
        Vector2 boxCenter = transform.position;
        Vector2 boxSize = new Vector2(1f, 1f);
        float angle = 0f;

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, angle);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyBase enemy = hit.GetComponent<EnemyBase>();
                enemy?.TakeDamage(damage);
            }
        }
        yield return new WaitForSeconds(2f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 size = new Vector2(1f, 1f);
        Gizmos.DrawWireCube(transform.position, size);
    }

}
