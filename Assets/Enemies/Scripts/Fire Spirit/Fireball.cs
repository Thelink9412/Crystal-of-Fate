using UnityEngine;

//Fire ball lanciata dal Fire Spirit
public class Fireball : MonoBehaviour
{
    #region Variables
    [SerializeField] private float speed = 5f;
    [SerializeField] private int damage = 10;
    private string currentState;
    private Animator animator;
    private bool hasHit = false;
    private Vector3 direction;
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
    #endregion
    #region Init and Update
    public void Init(Vector3 targetPosition)
    {
        direction = (targetPosition - transform.position).normalized;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
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
        ChangeAnimationState(FLY);
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (hasHit) return;
            hasHit = true;

            speed = 0;
            ChangeAnimationState(IMPACT);

            other.GetComponent<PlayerBase>()?.TakeDamage(damage);
            Destroy(gameObject, 0.4f);
        }
    }
    #endregion
}
