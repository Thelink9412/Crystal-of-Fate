using UnityEngine;

//Script per il firepoint che gestisce l'animazione di un fuoco
public class FirepointWizard : MonoBehaviour
{
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(Animate());
    }

    private System.Collections.IEnumerator Animate()
    {
        animator.Play("Firepoint");
        yield return new WaitForSeconds(2.3f);
        Destroy(gameObject);
    }
}
