using UnityEngine;
using System.Collections.Generic;

//Script per il PG Kitsune
public class Kitsune : PlayerBase
{
    #region Variables
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject firePoint;   //Punto da cui esce la fire ball
    [SerializeField] private Transform fireSpawnPoint;
    [SerializeField] private AudioClip[] sounds;
    private bool isAttacking = false;
    private float atkTimer = 0f;
    private string currentState;
    private Animator animator;
    private Dictionary<string, float> animationLengths = new Dictionary<string, float>();
    #endregion
    #region Animation
    const string IDLE = "Idle";
    const string ATTACK = "Attack";
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
    public void Init()
    {
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

        ChangeAnimationState(ATTACK);
        SoundFXManager.Instance.PlaySound(sounds[0], transform, 1f);
        yield return new WaitForSeconds(0.41f);
        StartCoroutine(Shoot(currentEnemy));

        atkTimer = 0f;
        isAttacking = false;
    }

    private System.Collections.IEnumerator Shoot(EnemyBase enemy)
    {
        if (fireballPrefab == null || firePoint == null)
            yield break;

        GameObject firepointKitsune = Instantiate(firePoint, fireSpawnPoint.position, Quaternion.identity);
        yield return new WaitForSeconds(0.3f);

        GameObject fireballKitsune = Instantiate(fireballPrefab, fireSpawnPoint.position, Quaternion.identity);
        FireballKitsune fbk = fireballKitsune.GetComponent<FireballKitsune>();
        fbk?.Init(enemy.transform, runtimeStats.attack, enemy);
    }

    //Abilità speciale: spara 9 proiettili, uno per ogni coda, in rapida successione a tutti i nemici in Round Robin
    protected override System.Collections.IEnumerator ActivateSpecialAbility()
    {
        isAttacking = true;
        abilityUI.DisableAbilityCd();
        float tick = 0.2f;
        ChangeAnimationState(SPECIAL);
        yield return new WaitForSeconds(0.44f);
        for (int i = 0; i < 9; i++)
        {
            List<EnemyBase> activeEnemies = roomManager.GetActiveEnemies();
            EnemyBase enemy = activeEnemies[i % activeEnemies.Count];
            SoundFXManager.Instance.PlaySound(sounds[0], transform, 1f);
            StartCoroutine(Shoot(enemy));
            yield return new WaitForSeconds(tick);
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
