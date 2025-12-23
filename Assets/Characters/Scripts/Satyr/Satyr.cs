using UnityEngine;
using System.Collections.Generic;

//Script per il personaggio Satyr
public class Satyr : PlayerBase
{
    #region Variables
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform fireSpawnPoint;
    [SerializeField] private AudioClip[] sounds;
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
    #region Init and Update
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
    #endregion
    #region Attacks
    private System.Collections.IEnumerator PerformAttack()
    {
        isAttacking = true;

        ChangeAnimationState(ATTACK);
        yield return new WaitForSeconds(0.43f);
        SoundFXManager.Instance.PlaySound(sounds[0], transform, 1f);
        StartCoroutine(Shoot());

        atkTimer = 0f;
        isAttacking = false;
    }

    private System.Collections.IEnumerator Shoot()
    {
        if (fireballPrefab == null || fireSpawnPoint == null || base.target == null)
            yield break;

        GameObject projectileSatyr = Instantiate(fireballPrefab, fireSpawnPoint.position, Quaternion.identity);
        ProjectileSatyr ps = projectileSatyr.GetComponent<ProjectileSatyr>();
        ps?.Init(base.target.position, runtimeStats.attack, this);
    }

    //Abilità speciale: cura tutti i PG di un ammontare di HP
    protected override System.Collections.IEnumerator ActivateSpecialAbility()
    {
        abilityUI.DisableAbilityCd();
        foreach (PlayerBase player in GameManager.Instance.GetActivePlayers())
            player.UpdateCurrentHealth(200f);

        SoundFXManager.Instance.PlaySound(sounds[1], transform, 1f);
        yield return new WaitForSeconds(1f);
        abilityUI.EnableAbilityCd();
    }
    #endregion
    protected override void Die()
    {
        ChangeAnimationState(DEATH);
        base.Die();
    }
}
