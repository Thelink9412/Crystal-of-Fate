using UnityEngine;
using UnityEngine.UI;

//Script per gestire le abilità dei PG e le relative UI
public class AbilityUI : MonoBehaviour
{
    #region Variables
    private PlayerBase player;
    [SerializeField] private Image abilityImage;
    [SerializeField] private Image fillBar;
    private Button abilityButton;
    private float timer = 0f;
    private bool abilityReady = false;
    private bool isAbilityCdActive = true;
    private Animator animator;
    #endregion
    #region Init and Update
    public void Initialize(PlayerBase assignedPlayer)
    {
        player = assignedPlayer;
        animator = GetComponent<Animator>();
        abilityButton = abilityImage.GetComponent<Button>();
        timer = 0f;
        abilityReady = false;
        UpdateUI();
    }

    void Update()
    {
        if (player == null || player.runtimeStats == null || abilityReady || !player.GetRoom().IsRoomActive() || !isAbilityCdActive || GameManager.Instance.IsPaused)
            return;

        timer += Time.deltaTime;
        if (timer >= player.runtimeStats.abilityCd)
        {
            abilityReady = true;
            animator.Play("Highlight");
            abilityButton.onClick.RemoveAllListeners();
            abilityButton.onClick.AddListener(() => ActivateAbility());
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        float fillAmount = timer / player.runtimeStats.abilityCd;
        fillBar.fillAmount = fillAmount;

        abilityImage.color = abilityReady
        ? new Color(1, 1, 1, 1)
        : new Color(1, 1, 1, 0.3f);
    }
    #endregion
    private void ActivateAbility()
    {
        if (!abilityReady || player == null)
            return;

        if (player.TryActivateSpecial())
        {
            abilityReady = false;
            timer = 0;
            animator.Play("Still");
            abilityImage.color = new Color(1, 1, 1, 0.3f);
        }
    }

    public void EnableAbilityCd()
    {
        isAbilityCdActive = true;
    }

    public void DisableAbilityCd()
    {
        isAbilityCdActive = false;
    }

    public PlayerBase GetPlayerOfUI()
    {
        return player;
    }
}
