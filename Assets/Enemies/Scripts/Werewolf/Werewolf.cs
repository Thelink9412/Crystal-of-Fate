using UnityEngine;
using System.Collections.Generic;

//Script per il nemico Werewolf, quando spawna carica e attacca in corsa infliggendo più danni
public class Werewolf : EnemyBase
{
    #region Variables
    [SerializeField] private float atkCooldown = 2f;
    [SerializeField] private int jumpAttackDamage;

    private Animator animator;
    private Dictionary<string, float> animationLengths = new Dictionary<string, float>();
    [SerializeField] private AudioClip[] sounds;

    private string currentState;
    private bool isAttacking;
    private bool hasArrived = false;
    private float atkTimer = 0f;
    #endregion

    #region Animations
    const string IDLE = "Idle";
    const string ATTACK = "Attack";
    const string JUMP_ATTACK = "Jump Attack";
    const string RUN = "Run";
    const string DEATH = "Death";

    void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        animator.Play(newState);

        currentState = newState;
    }

    private void CacheAnimationLengths()
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            animationLengths[clip.name] = clip.length;
        }
    }

    float GetAnimationLength(string animationName)
    {
        if (animationLengths.TryGetValue(animationName, out float length))
            return length;

        Debug.LogWarning("Animazione non trovata: " + animationName);
        return 0f;
    }
    #endregion
    #region Init and Update
    public void Init()
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player").transform;

        base.Init(target.position);
    }

    protected void Start()
    {
        animator = GetComponent<Animator>();
        CacheAnimationLengths();
        base.Start();
        Init();
    }

    void Update()
    {
        if (GameManager.Instance.IsPaused)
        {
            if (animator.speed != 0)
                animator.speed = 0;
            return;
        }
        else
        {
            if (animator.speed != 1)
                animator.speed = 1;
        }
        if (!isAttacking && !GameManager.Instance.IsGameOver)
        {
            base.Update();
            atkTimer += Time.deltaTime;
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance >= minRangedDistance)
                ChangeAnimationState(RUN);
            else
            {
                if (!hasArrived)
                    StartCoroutine(JumpAttack());

                hasArrived = true;
                ChangeAnimationState(IDLE);
                if (atkTimer >= atkCooldown)
                {
                    isAttacking = true;
                    StartCoroutine(PerformAttack());
                }
            }
        }
    }
    #endregion
    #region Attacks
    private System.Collections.IEnumerator PerformAttack()
    {
        ChangeAnimationState(ATTACK);
        SoundFXManager.Instance.PlaySound(sounds[1], transform, 1f);
        yield return new WaitForSeconds(GetAnimationLength(ATTACK));
        if (target.CompareTag("Player"))
        {
            PlayerBase player = target.GetComponent<PlayerBase>();
            player?.TakeDamage(stats.damage);
        }
        atkTimer = 0f;
        isAttacking = false;
    }

    private System.Collections.IEnumerator JumpAttack()
    {
        ChangeAnimationState(JUMP_ATTACK);
        SoundFXManager.Instance.PlaySound(sounds[0], transform, 1f);
        while (GameManager.Instance.IsPaused)
            yield return null;

        yield return new WaitForSeconds(GetAnimationLength(JUMP_ATTACK));
        if (target.CompareTag("Player"))
        {
            PlayerBase player = target.GetComponent<PlayerBase>();
            player?.TakeDamage(jumpAttackDamage);
        }
        atkTimer = 0f;
        isAttacking = false;
    }
    #endregion
    protected override void Die()
    {
        StartCoroutine(WerewolfDie());
    }

    private System.Collections.IEnumerator WerewolfDie()
    {
        SetActiveFalse();
        ChangeAnimationState(DEATH);
        SoundFXManager.Instance.PlaySound(sounds[2], transform, 1f);
        while (GameManager.Instance.IsPaused)
            yield return null;
            
        yield return new WaitForSeconds(GetAnimationLength(DEATH));
        base.Die();
    }
}
