using UnityEngine;
using UnityEngine.UI;

//Script per gestire la stanza con le spine; si possono attivare soltanto una volta e a partire dal secondo round
//Questa stanza può essere spawnata soltanto dal livello 3 in poi
public class SpikeRoomManager : RoomManager
{
    #region Variables
    [SerializeField] private Spikes[] spikesInScene;
    [SerializeField] private Image spikesImage;
    [SerializeField] private Image border;
    [SerializeField] private Image fillBar;
    [SerializeField] private Button spikeButton;
    [SerializeField] private float fillTime = 20f;
    [SerializeField] private float spikeDamage = 30f;

    private Animator animator;

    private bool activated = false;
    private float spikeTimer = 0f;
    private bool buttonIsFilling = true;
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

        spikeTimer += Time.deltaTime;

        if (spikeTimer >= fillTime)
        {
            buttonIsFilling = false;
            animator.Play("Highlight");
            spikeButton.onClick.RemoveAllListeners();
            spikeButton.onClick.AddListener(ActivateSpikes);
        }

        UpdateUI();
    }

    void ActivateSpikes()
    {
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused || !IsRoomActive() || activated)
            return;

        foreach (var s in spikesInScene)
            StartCoroutine(s.ActivateSpikes(spikeDamage));

        spikesImage.fillAmount = 0;
        activated = true;
        animator.Play("Still");
        spikesImage.color = new Color(1, 1, 1, 0.3f);
        spikeButton.onClick.RemoveAllListeners();
    }
    
    private void UpdateUI()
    {
        float fillAmount = spikeTimer / fillTime;
        fillBar.fillAmount = fillAmount;

        spikesImage.color = !buttonIsFilling
        ? new Color(1, 1, 1, 1)
        : new Color(1, 1, 1, 0.3f);
    }
}
