using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Script necessario per passare al GameManager le informazioni necessarie per gestire
//l'interfaccia utente del livello e permettere i riferimenti cross-scene
public class LevelUIList : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pauseUI;
    public GameObject optionsUI;
    public GameObject roundPopupUI;
    public GameObject endLevelPopupUI;
    public GameObject gameOverPopupUI;
    public GameObject confirmStatsPopupUI;
    public GameObject currencyUI;
    public TextMeshProUGUI roundUIText;
    [Header("Buttons")]
    public Button startButton;
    public Button nextLevelButton;
    public Button confirmStatsButton;
    public Button confirmInitialChoicesButton;
    public Button showEnemySpawnStatsButton;
    public Button gameOverButton;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button optionsButton;
    public Button exitOptionsButton;
    public Button exitButton;

    void Start()
    {
        GameManager.Instance.SetLevelUI(this);
        CurrencyManager.Instance.SetUI(this);
    }
}
