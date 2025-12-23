using UnityEngine;
using System.Collections.Generic;

//Script per il personaggio Samurai Archer
public class SamuraiArcher : PlayerBase
{
    #region Variables
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private AudioClip[] sounds;
    private bool isAttacking = false;
    private float actualAttackSpeed;
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
    #region Init and Update
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        CacheAnimationLengths();
        base.Start();
        base.Init();
        actualAttackSpeed = runtimeStats.atkCd;
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
            if (roomManager.HasEnemies())
                FindTarget();
            
            else
                target = null;
            

            if (target == null || !currentEnemy.GetActive()) return;

            if (!isAttacking)
            {
                ChangeAnimationState(IDLE);
                if (atkTimer >= actualAttackSpeed)
                    StartCoroutine(PerformAttack());
                
            }
        }
    }
    #endregion
    #region Attacks
    private System.Collections.IEnumerator PerformAttack()
    {
        isAttacking = true;

        ChangeAnimationState(ATTACK);
        yield return new WaitForSeconds(1.07f);
        SoundFXManager.Instance.PlaySound(sounds[0], transform, 1f);
        Shoot();

        atkTimer = 0f;
        isAttacking = false;
    }

    void Shoot()
    {
        if (arrowPrefab == null || firePoint == null || base.target == null)
            return;

        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        Arrow arr = arrow.GetComponent<Arrow>();
        arr?.Init(base.target.position, runtimeStats.attack, this);
    }

    //Abilità speciale: raddoppia la velocità d'attacco per un breve periodo
    protected override System.Collections.IEnumerator ActivateSpecialAbility()
    {
        abilityUI.DisableAbilityCd();
        SoundFXManager.Instance.PlaySound(sounds[1], transform, 1f);
        float atkSpeed = actualAttackSpeed;
        actualAttackSpeed /= 2;
        yield return new WaitForSeconds(10f);
        actualAttackSpeed = atkSpeed;
        abilityUI.EnableAbilityCd();
    }
    #endregion
    protected override void Die()
    {
        ChangeAnimationState(DEATH);
        base.Die();
    }
}
