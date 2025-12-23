using UnityEngine;
using UnityEngine.UI;

//Script per gestire la stanza con la botola essa può essere attivata soltanto una volta e a partire dal secondo
//round; quando attivata, essa one-shotta qualsiasi nemico sopra di essa
//I nemici morti in questo modo non rilasciano loot
//Questa stanza può essere spawnata soltanto dal livello 3 in poi
public class HatchRoomManager : RoomManager
{
    #region Variables
    [SerializeField] private Hatch hatchInScene;
    [SerializeField] private Image hatchImage;
    [SerializeField] private Image border;
    [SerializeField] private Image fillBar;
    [SerializeField] private Button hatchButton;
    [SerializeField] private float fillTime = 20f;

    private bool activated = false;
    private float hatchTimer = 0f;
    private bool buttonIsFilling = true;
    private Animator animator;
    #endregion
    protected override void Start()
    {
        animator = border.gameObject.GetComponent<Animator>();
        base.Start();
    }

    void Update()
    {
        if (!buttonIsFilling || currentRound <= 1 || activated || GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)
            return;

        hatchTimer += Time.deltaTime;

        if (hatchTimer >= fillTime)
        {
            buttonIsFilling = false;
            animator.Play("Highlight");
            hatchButton.onClick.RemoveAllListeners();
            hatchButton.onClick.AddListener(ActivateHatch);
        }

        UpdateUI();
    }

    void ActivateHatch()
    {
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused || activated || !IsRoomActive())
            return;

        StartCoroutine(hatchInScene.OpenHatch());

        hatchImage.fillAmount = 0;
        activated = true;
        animator.Play("Still");
        hatchImage.color = new Color(1, 1, 1, 0.3f);
        hatchButton.onClick.RemoveAllListeners();
    }

    private void UpdateUI()
    {
        float fillAmount = hatchTimer / fillTime;
        fillBar.fillAmount = fillAmount;

        hatchImage.color = !buttonIsFilling
        ? new Color(1, 1, 1, 1)
        : new Color(1, 1, 1, 0.3f);
    }
}
