using UnityEngine;

//Script per gestire l'apertura della botola
public class Hatch : MonoBehaviour
{
    public Sprite hatchOpen;
    public Sprite hatchClose;
    private SpriteRenderer sr;
    private bool isOpen = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public System.Collections.IEnumerator OpenHatch()
    {
        isOpen = true;
        Vector2 boxCenter = transform.position;
        Vector2 boxSize = new Vector2(1.15f, 1.15f);
        float angle = 0f;

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, angle);
        sr.sprite = hatchOpen;
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log("Il nemico è sopra la botola!");
                EnemyBase enemy = hit.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.DieByHatch();
                    Debug.Log("Il nemico è caduto nella botola!");
                }
            }
        }
        yield return new WaitForSeconds(3f);
        sr.sprite = hatchClose;
        isOpen = false;
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 size = new Vector2(1.15f, 1.15f);
        Gizmos.DrawWireCube(transform.position, size);
    }

}
