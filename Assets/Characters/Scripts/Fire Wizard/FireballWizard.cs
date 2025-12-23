using UnityEngine;
using System.Collections.Generic;

//Script per la fire ball dello stregone
public class FireballWizard : MonoBehaviour
{
    #region Variables
    [SerializeField] private float speed = 5f;
    private float damage;
    private string currentState;
    private Animator animator;
    private Dictionary<string, float> animationLengths = new Dictionary<string, float>();

    private Transform target;
    private EnemyBase actualTarget;
    #endregion

    #region Animations
    const string FLY = "Fly";
    const string IMPACT = "Impact";

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
    public void Init(Vector3 targetPosition, float d, FireWizard fireWizard)
    {
        PlayerBase player = fireWizard.GetComponent<PlayerBase>();
        if (player != null && player.currentEnemy != null)
        {
            target = player.currentEnemy.transform;
            actualTarget = player.currentEnemy;
            damage = d;
        }
        else
            Destroy(gameObject);
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        CacheAnimationLengths();
    }

    void Update()
    {
        if (target == null)
        {
            Debug.Log("Target Null!");
            Destroy(gameObject);
            return;
        }
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
        ChangeAnimationState(FLY);
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy == null || enemy != actualTarget)
            return;

        speed = 0;
        StartCoroutine(DamageEnemy(enemy));
    }

    private System.Collections.IEnumerator DamageEnemy(EnemyBase enemy)
    {
        ChangeAnimationState(IMPACT);
        yield return new WaitForSeconds(GetAnimationLength(IMPACT));
        enemy.TakeDamage(damage);
        Destroy(gameObject);
    }
    #endregion
}
