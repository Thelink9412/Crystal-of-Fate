using UnityEngine;
using System.Collections.Generic;

//Script per gestire il PG Samurai Commandant
public class SamuraiCommandant : PlayerBase
{
    #region Variables
    [SerializeField] protected float meleeRange = 2f;

    [SerializeField] private AudioClip[] sounds;
    private bool isAttacking = false;
    private float atkTimer = 0f;
    private string currentState;
    private Animator animator;
    private Dictionary<string, float> animationLengths = new Dictionary<string, float>();
    #endregion

    #region Animations
    //Lista per gestire l'attacco speciale
    private List<string> attackAnimation = new List<string> { "Attack1", "Attack2", "Attack3" };
    const string IDLE = "Idle";
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
        ChangeAnimationState(attackAnimation[1]);
        SoundFXManager.Instance.PlaySound(sounds[0], transform, 1f);
        yield return new WaitForSeconds(GetAnimationLength(attackAnimation[1]));
        foreach (EnemyBase enemy in enemies)
        {
            enemy.TakeDamage(runtimeStats.attack);
        }

        atkTimer = 0f;
        isAttacking = false;
    }

    //Abilità speciale: attiva una combo di 6 attacchi melee in rapida successione
    protected override System.Collections.IEnumerator ActivateSpecialAbility()
    {
        isAttacking = true;
        abilityUI.DisableAbilityCd();
        float tick = 0.4f;
        for (int i = 0; i < 2; i++)
        {
            foreach (string anim in attackAnimation)
            {
                Debug.Log("Performo: " + anim);
                ChangeAnimationState(anim);
                SoundFXManager.Instance.PlaySound(sounds[0], transform, 1f);
                yield return new WaitForSeconds(tick);
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 3f);
                foreach (Collider2D col in colliders)
                    if (col.CompareTag("Enemy"))
                    {
                        EnemyBase enemy = col.GetComponent<EnemyBase>();
                        if (enemy != null && enemy.GetActive())
                            enemy.TakeDamage(runtimeStats.attack);

                    }
            }
        }

        isAttacking = false;
        abilityUI.EnableAbilityCd();
    }
    #endregion
    protected override void Die()
    {
        ChangeAnimationState(DEATH);
        base.Die();
    }
}
