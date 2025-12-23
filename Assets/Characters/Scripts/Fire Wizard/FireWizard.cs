using UnityEngine;
using System.Collections.Generic;

//Il primo personaggio dal player, uno stregone che lancia palle di fuoco
public class FireWizard : PlayerBase
{
    #region Variables
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject firePointPrefab;
    [SerializeField] private Transform firePoint;   //Punto da cui esce la fire ball
    [SerializeField] private AudioClip[] sounds;
    private bool isAttacking = false;
    private float atkTimer = 0f;
    private string currentState;
    private Animator animator;
    private Dictionary<string, float> animationLengths = new Dictionary<string, float>();
    #endregion
    
    #region Animations
    const string IDLE = "Idle";
    const string FIREBALL = "Fireball";
    const string DEATH = "Death";
    const string SPECIAL = "Special";

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
    #region Init
    public void Init(Vector3 targetPosition)
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Enemy").transform;

        base.Init();
    }

    protected override void Start()
    {
        animator = GetComponent<Animator>();
        CacheAnimationLengths();
        base.Start();
        Init();
    }
    #endregion
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
            {
                FindTarget();
            }
            else
            {
                target = null;
            }

            if (target == null || !currentEnemy.GetActive()) return;

            if (!isAttacking)
            {
                ChangeAnimationState(IDLE);
                if (atkTimer >= runtimeStats.atkCd)
                {
                    StartCoroutine(PerformAttack());
                }
            }
        }
    }
    #region Attacks
    private System.Collections.IEnumerator PerformAttack()
    {
        isAttacking = true;

        ChangeAnimationState(FIREBALL);
        SoundFXManager.Instance.PlaySound(sounds[0], transform, 1f);
        yield return new WaitForSeconds(GetAnimationLength(FIREBALL));
        while (GameManager.Instance.IsPaused)
            yield return null;
        Shoot();

        atkTimer = 0f;
        isAttacking = false;
    }

    void Shoot()
    {
        if (fireballPrefab == null || firePoint == null || base.target == null)
            return;

        GameObject fireballWizard = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        FireballWizard fbw = fireballWizard.GetComponent<FireballWizard>();
        fbw?.Init(base.target.position, runtimeStats.attack, this);
    }

    //Abilità speciale: lancia un raggio di fuoco che danneggia nel tempo i nemici vicini
    protected override System.Collections.IEnumerator ActivateSpecialAbility()
    {
        isAttacking = true;
        abilityUI.DisableAbilityCd();
        float damagePerTick = 15f;
        float tick = 0.5f;
        ChangeAnimationState(SPECIAL);
        yield return new WaitForSeconds(0.15f);
        GameObject firepointWizard = Instantiate(firePointPrefab, firePoint.position, Quaternion.identity);
        SoundFXManager.Instance.PlaySound(sounds[1], transform, 1f);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 3f);

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                EnemyBase enemy = col.GetComponent<EnemyBase>();
                if (enemy != null && enemy.GetActive())
                    enemy.TakeFireSpecialDamage(2f, tick, damagePerTick);

            }
        }
        //SoundFXManager.Instance.StopAudio();
        yield return new WaitForSeconds(2.15f);
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
