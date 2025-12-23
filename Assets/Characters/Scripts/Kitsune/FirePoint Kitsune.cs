using UnityEngine;

//Script per gestire il firepoint del PG Kitsune, necessario per una migliore resa visiva
public class FirePointKitsune : MonoBehaviour
{
    private Animator animator;
    void Start()
    {
        Debug.Log("Istanzio firepoint");
        animator = GetComponent<Animator>();
        StartCoroutine(Animate());
    }

    private System.Collections.IEnumerator Animate()
    {
        animator.Play("Firepoint");
        yield return new WaitForSeconds(0.4f);
        Destroy(gameObject);
    }
}
