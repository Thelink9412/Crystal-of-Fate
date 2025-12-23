using UnityEngine;
using System.Collections.Generic;

//Nemico Plent, attacca da vicino con una nube di veleno che danneggia il player nel tempo
public class Plent : EnemyBase
{
    #region Variables
    [SerializeField] private GameObject poison;
    [SerializeField] private Transform firePoint;   //Punto da cui esce la nube di veleno
    [SerializeField] private float atkCooldown = 2f;
    [SerializeField] private float poisonTick = 2f;
    [SerializeField] private float poisonDuration = 5f;
    [SerializeField] private float poisonDamage = 8f;
    [SerializeField] private AudioClip[] sounds;
    private Dictionary<string, float> animationLengths = new Dictionary<string, float>();
    private Animator animator;
    private string currentState;
    private bool isAttacking;
    private float atkTimer = 0f;
    #endregion

    #region Animations
    const string IDLE = "Idle";
    const string POISON = "Poison";
    const string WALK = "Walk";
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
        ChangeAnimationState(POISON);
        SoundFXManager.Instance.PlaySound(sounds[0], transform, 1f);
        while (GameManager.Instance.IsPaused)
            yield return null;

        yield return new WaitForSeconds(GetAnimationLength(POISON));
        PoisonCloud();
        atkTimer = 0f;
        isAttacking = false;
    }

    private void PoisonCloud()
    {
        if (poison == null || firePoint == null || base.target == null)
            return;

        GameObject poisonCloud = Instantiate(poison, firePoint.position, Quaternion.identity);
        PoisonCloud pc = poisonCloud.GetComponent<PoisonCloud>();
        pc?.Init(poisonTick, poisonDuration, poisonDamage);
    }
    #endregion
    protected override void Die()
    {
        SetActiveFalse();
        ChangeAnimationState(DEATH);
        base.Die();
    }
}
