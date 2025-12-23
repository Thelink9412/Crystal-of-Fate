using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Script per i collegamenti della UI del Main Menu con il GameManager
public class TitleScreenUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject gameInfoUI;
    public GameObject optionsUI;
    [Header("Text Elements")]
    public TextMeshProUGUI gameInfoText;
    public TextMeshProUGUI continueText;
    [Header("Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button loadGameButton;
    public Button optionsButton;
    public Button exitOptionsButton;
    public Button exitButton;

    void Start()
    {
        GameManager.Instance.SetTitleScreenUI(this);
    }
}
