using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Script per settare la UI del tutorial e permettere collegamenti con il GameManager cross-scene
public class TutorialUI : MonoBehaviour
{
    public GameObject[] slides;
    [Header("Buttons")]
    public Button nextButton;
    public Button previousButton;
    public Button startGameButton;

    void Start()
    {
        GameManager.Instance.SetTutorialUI(this);
    }
}
