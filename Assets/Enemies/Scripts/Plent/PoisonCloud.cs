using UnityEngine;

//Nube di veleno del nemico Plent
public class PoisonCloud : MonoBehaviour
{
    private float poisonTick;
    private float poisonDuration;
    private float poisonDamage;
    private Animator animator;
    public void Init(float pt, float pdu, float pda)
    {
        poisonTick = pt;
        poisonDuration = pdu;
        poisonDamage = pda;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.Play("Cloud");
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerBase player = playerObj.GetComponent<PlayerBase>();
            player?.TakePoisonDamage(poisonTick, poisonDuration, poisonDamage);
        }
        Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
    }
}