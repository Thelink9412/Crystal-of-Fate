using UnityEngine;
using System.Collections.Generic;

//Script per il PG Knight
public class Knight : PlayerBase
{
    #region Variables
    [SerializeField] protected float meleeRange = 2f;
    [SerializeField] private GameObject particles;   //Particelle dell'attacco
    [SerializeField] private Transform particlesSpawnPoint;
    [SerializeField] private AudioClip[] sounds;
    [SerializeField] private float reductionDamage;     //Moltiplicatore per ridurre i danni subiti
    private float actualReductionDamage = 0;
    private bool isAttacking = false;
    private float atkTimer = 0f;
    private string currentState;
    private Animator animator;
    private Dictionary<string, float> animationLengths = new Dictionary<string, float>();
    #endregion

    #region Animations
    const string IDLE = "Idle";
    const string ATTACK = "Attack";
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
    #region Start and Update
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        CacheAnimationLengths();
        base.Init();
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
        if (!IsStunned() && !GameManager.Instance.IsGameOver)
        {
            atkTimer += Time.deltaTime;
            if (!isAttacking)
            {
                ChangeAnimationState(IDLE);
                if (atkTimer >= runtimeStats.atkCd)
                {
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, meleeRange);
                    List<EnemyBase> enemiesInRange = new List<EnemyBase>();

                    foreach (Collider2D col in colliders)
                    {
                        if (col.CompareTag("Enemy"))
                        {
                            EnemyBase enemy = col.GetComponent<EnemyBase>();
                            if (enemy != null && enemy.GetActive())
                            {
                                enemiesInRange.Add(enemy);
                            }
                        }
                    }

                    if (enemiesInRange.Count > 0)
                    {
                        StartCoroutine(PerformMeleeAttack(enemiesInRange));
                    }
                }
            }
        }
    }
    #endregion
    #region Attacks
    private System.Collections.IEnumerator PerformMeleeAttack(List<EnemyBase> enemies)
    {
        isAttacking = true;
        ChangeAnimationState(ATTACK);
        SoundFXManager.Instance.PlaySound(sounds[0], transform, 1f);
        yield return new WaitForSeconds(0.55f);
        StartCoroutine(SpawnParticles());
        foreach (EnemyBase enemy in enemies)
        {
            enemy.TakeDamage(runtimeStats.attack);
        }

        atkTimer = 0f;
        isAttacking = false;
    }

    private System.Collections.IEnumerator SpawnParticles()
    {
        if (particles == null || particlesSpawnPoint == null)
            yield break;

        GameObject particlesToSpawn = Instantiate(particles, particlesSpawnPoint.position, Quaternion.identity);
    }

    //Abilità speciale: riduce i danni subiti per un periodo di tempo
    protected override System.Collections.IEnumerator ActivateSpecialAbility()
    {
        abilityUI.DisableAbilityCd();
        SoundFXManager.Instance.PlaySound(sounds[1], transform, 1f);
        actualReductionDamage = reductionDamage;
        yield return new WaitForSeconds(8f);
        actualReductionDamage = 0;
        abilityUI.EnableAbilityCd();
    }
    #endregion
    public override void TakeDamage(float damage)
    {
        float actualDamage = damage * (1 - actualReductionDamage);
        base.TakeDamage(actualDamage);
    }

    protected override void Die()
    {
        ChangeAnimationState(DEATH);
        base.Die();
    }
}
