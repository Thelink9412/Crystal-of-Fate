using UnityEngine;
using System.Collections.Generic;

//Script per il nemico yokai Yamabushi, farà una combo di 3 attacchi
public class Yamabushi : EnemyBase
{
    #region Variables
    [SerializeField] private float atkCooldown = 2f;
    [SerializeField] private float finalAttackDamage;

    private Animator animator;
    private Dictionary<string, float> animationLengths = new Dictionary<string, float>();
    [SerializeField] private AudioClip[] sounds;

    private string currentState;
    private bool isAttacking;
    private float atkTimer = 0f;
    #endregion

    #region Animations
    const string IDLE = "Idle";
    private enum ATTACKS
    {
        ATTACK_1,
        ATTACK_2,
        ATTACK_3
    }
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
                    isAttacking = true;
                    StartCoroutine(PerformCombo());
                }
            }
        }
    }
    #endregion
    #region Combo
    private System.Collections.IEnumerator PerformCombo()
    {
        foreach (ATTACKS attack in System.Enum.GetValues(typeof(ATTACKS)))
        {
            string attackName = GetAttackName(attack);
            ChangeAnimationState(attackName);
            SoundFXManager.Instance.PlaySound(sounds[0], transform, 1f);
            while (GameManager.Instance.IsPaused)
                yield return null;

            yield return new WaitForSeconds(GetAnimationLength(attackName));
            if (target.CompareTag("Player"))
            {
                PlayerBase player = target.GetComponent<PlayerBase>();
                player?.TakeDamage((attackName == "Attack 3") ? finalAttackDamage : stats.damage);
            }
        }
        atkTimer = 0f;
        isAttacking = false;
    }

    private string GetAttackName(ATTACKS attack)
    {
        return attack switch
        {
            ATTACKS.ATTACK_1 => "Attack 1",
            ATTACKS.ATTACK_2 => "Attack 2",
            ATTACKS.ATTACK_3 => "Attack 3",
            _ => "Unknown",
        };
    }
    #endregion
    protected override void Die()
    {
        StartCoroutine(YamabushiDie());
    }

    private System.Collections.IEnumerator YamabushiDie()
    {
        SetActiveFalse();
        ChangeAnimationState(DEATH);
        SoundFXManager.Instance.PlaySound(sounds[1], transform, 1f);
        while (GameManager.Instance.IsPaused)
            yield return null;

        yield return new WaitForSeconds(GetAnimationLength(DEATH));
        base.Die();
    }
}
