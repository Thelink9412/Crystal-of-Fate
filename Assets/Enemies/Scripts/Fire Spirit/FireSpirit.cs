using UnityEngine;
using System.Collections.Generic;

//Nemico Fire Spirit: lancia delle fire ball dalla distanza e attacca melee da vicino
//Quando muore esplode facendo danni sia a nemici che al player circostanti
public class FireSpirit : EnemyBase
{
    #region Variables
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform firePoint;   // Punto da cui esce la fire ball
    [SerializeField] private float atkCooldown = 2f;
    [SerializeField] private AudioClip[] sounds;

    private Animator animator;
    private string currentState;
    private Dictionary<string, float> animationLengths = new Dictionary<string, float>();

    private bool isAttacking;
    private bool isExploding = false;
    private float atkTimer = 0f;
    #endregion

    #region Animations
    const string ATTACK = "Attack";
    const string EXPLOSION = "Explotion";
    const string IDLE = "Idle";
    const string SHOT = "Shot";

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
        if (!isAttacking && !isExploding && !GameManager.Instance.IsGameOver)
        {
            base.Update();
            ChangeAnimationState(IDLE);

            atkTimer += Time.deltaTime;
            if (atkTimer >= atkCooldown)
            {
                StartCoroutine(PerformAttack());
            }
        }
    }
    #endregion
    #region Attacks
    private System.Collections.IEnumerator PerformAttack()
    {
        isAttacking = true;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance >= minRangedDistance)
        {
            ChangeAnimationState(SHOT);
            SoundFXManager.Instance.PlaySound(sounds[0], transform, 1f);
            while (GameManager.Instance.IsPaused)
                yield return null;

            yield return new WaitForSeconds(GetAnimationLength(SHOT));
            Shoot();
        }
        else
        {
            ChangeAnimationState(ATTACK);
            SoundFXManager.Instance.PlaySound(sounds[1], transform, 1f);
            while (GameManager.Instance.IsPaused)
                yield return null;

            yield return new WaitForSeconds(GetAnimationLength(ATTACK));
            MeleeAttack();
        }

        atkTimer = 0f;
        isAttacking = false;
    }

    void Shoot()
    {
        if (fireballPrefab == null || firePoint == null || base.target == null)
            return;

        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        Fireball fb = fireball.GetComponent<Fireball>();
        fb?.Init(base.target.position);
    }

    void MeleeAttack()
    {
        Debug.Log("Attacco corpo a corpo!");
        if (target.CompareTag("Player"))
        {
            PlayerBase player = target.GetComponent<PlayerBase>();
            player?.TakeDamage(15);
        }
    }

    private void Explode()
    {
        float explosionRadius = 3f;
        int explosionDamage = 20;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerBase player = hit.GetComponent<PlayerBase>();
                player?.TakeDamage(explosionDamage);
            }

            if (hit.CompareTag("Enemy"))
            {
                EnemyBase enemy = hit.GetComponent<EnemyBase>();
                if (enemy != null && enemy != this)
                    enemy.TakeDamage(explosionDamage);
                
            }
        }
    }
    #endregion
    protected override void Die()
    {
        SetActiveFalse();
        StartCoroutine(ExplodeThanDie());
    }

    private System.Collections.IEnumerator ExplodeThanDie()
    {
        isExploding = true;
        ChangeAnimationState(EXPLOSION);
        SoundFXManager.Instance.PlaySound(sounds[2], transform, 1f);
        yield return new WaitForSeconds(GetAnimationLength(EXPLOSION));
        Explode();
        base.Die();
    }
}
