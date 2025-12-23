using UnityEngine;

public class SwordParticles : MonoBehaviour
{
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(Animate());
    }

    private System.Collections.IEnumerator Animate()
    {
        animator.Play("Spawn");
        yield return new WaitForSeconds(0.45f);
        Destroy(gameObject);
    }
}
