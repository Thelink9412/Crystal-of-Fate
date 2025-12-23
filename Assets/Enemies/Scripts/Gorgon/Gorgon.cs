using UnityEngine;
using System.Collections.Generic;

//Script per il nemico Gorgon, la sua abilità è quella di stunnare il player
public class Gorgon : EnemyBase
{
    #region Variables
    [SerializeField] private GameObject light;
    [SerializeField] private Transform firePoint;   //Punto da cui esce la nube di veleno
    [SerializeField] private float atkCooldown = 2f;
    [SerializeField] private float stunDuration = 3f;
    [SerializeField] private AudioClip[] sounds;
    [SerializeField] private int attackBeforeSpecial;
    private Animator animator;
    private string currentState;
    private Dictionary<string, float> animationLengths = new Dictionary<string, float>();
    private bool isAttacking;
    private float atkTimer = 0f;
    private int attacks = 0;
    #endregion

    #region Animations
    const string IDLE = "Idle";
    const string SPECIAL = "Special";
    const string WALK = "Walk";
    const string DEATH = "Death";
    const string ATTACK = "Attack";

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
                ChangeAnimationState(WALK);

            else
            {
                ChangeAnimationState(IDLE);
                if (atkTimer >= atkCooldown)
                {
                    StartCoroutine(PerformAttack());
                }
            }
        }
    }
    #endregion
    #region Attacks
    private System.Collections.IEnumerator PerformAttack()
    {
        isAttacking = true;
        while (GameManager.Instance.IsPaused)
            yield return null;

        if (attacks == attackBeforeSpecial)
        {
            Debug.Log("Attacco speciale di Gorgon: stunno il player");
            ChangeAnimationState(SPECIAL);
            SoundFXManager.Instance.PlaySound(sounds[1], transform, 1f);
            yield return new WaitForSeconds(GetAnimationLength(SPECIAL));
            SpecialAttack();
            attacks = 0;
        }
        else
        {
            ChangeAnimationState(ATTACK);
            SoundFXManager.Instance.PlaySound(sounds[0], transform, 1f);
            yield return new WaitForSeconds(GetAnimationLength(ATTACK));
            if (target.CompareTag("Player"))
            {
                PlayerBase player = target.GetComponent<PlayerBase>();
                player?.TakeDamage(stats.damage);
            }
            attacks++;
        }
        atkTimer = 0f;
        isAttacking = false;
    }

    private void SpecialAttack()
    {
        if (light == null || firePoint == null || base.target == null)
            return;

        GameObject lightRay = Instantiate(light, firePoint.position, Quaternion.identity);
        LightRay lr = lightRay.GetComponent<LightRay>();
        lr?.Init(stunDuration);
    }
    #endregion
    protected override void Die()
    {
        SetActiveFalse();
        StartCoroutine(GorgonDie());
    }

    private System.Collections.IEnumerator GorgonDie()
    {
        ChangeAnimationState(DEATH);
        yield return new WaitForSeconds(GetAnimationLength(DEATH));
        base.Die();
    }
}
