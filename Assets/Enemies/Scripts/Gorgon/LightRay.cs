using UnityEngine;

//Raggio di luce che stunna il player lanciato dalla Gorgon
public class LightRay : MonoBehaviour
{
    private string currentState;
    private Animator animator;
    private float stunDuration;

    public void Init(float sd)
    {
        animator = GetComponent<Animator>();
        stunDuration = sd;
    }

    void Start()
    {
        animator.Play("Fly");
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerBase player = playerObj.GetComponent<PlayerBase>();
            player?.ApplyStun(stunDuration);
        }
        Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
    }
}
