using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Script per la gestione della scelta del PG e relative UI; essa avviene soltanto all'inizio del livello, non si
//può cambiare dopo; i PG vengono sbloccati con l'avanzare nei livelli, nel primo sarà disponibile soltanto il 
//Fire Wizard, dal livello 2 Knight e Archer e dal livello 3 Satyr, Kitsune e Commandant; nella UI i PG che si
//potranno scegliere avranno il testo in bianco e nessun icona a fianco, i PG bloccati dal livello avranno un'icona
//a forma di lucchetto e testo in grigio, i PG scelti avranno testo in grigio e icona del pollice in su (nel set di
//icone non ho trovato di meglio :) ), mentre il PG scelto per la relativa stanza avrà una freccia accanto
public class ChoosePGUI : MonoBehaviour
{
    #region Variables
    [SerializeField] private Button[] characterButtons;
    [SerializeField] private TextMeshProUGUI[] texts;
    [SerializeField] private Image[] icons;
    [SerializeField] private Image[] arrows;
    [SerializeField] private Sprite locked;
    [SerializeField] private Sprite selected;
    private RoomManager room;
    private TextMeshProUGUI characterSelected;
    private Dictionary<TextMeshProUGUI, bool> isAvailable = new Dictionary<TextMeshProUGUI, bool>();
    private Dictionary<TextMeshProUGUI, bool> isSelected = new Dictionary<TextMeshProUGUI, bool>();
    #endregion
    #region Initialization
    public void Initialize(RoomManager r)
    {
        room = r;
        PopolateDictionaries();
        AddListeners();
        UpdateUI();
    }

    private void AddListeners()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;
            characterButtons[index].onClick.AddListener(() => SelectPG(index));
        }
    }

    private void PopolateDictionaries()
    {
        for (int i = 0; i < texts.Length; i++)
        {
            isAvailable.Add(texts[i], i == 0);
            isSelected.Add(texts[i], false);
        }
    }
    #endregion
    #region UI Management
    private void UpdateUI()
    {
        UpdateTextAndIcon(0);
        arrows[0].color = new Color(1, 1, 1, characterSelected == texts[0] ? 1f : 0f);
        for (int i = 1; i <= 2; i++)
        {
            isAvailable[texts[i]] = GameManager.Instance.Level > 1;
            arrows[i].color = new Color(1, 1, 1, 0f);
            UpdateTextAndIcon(i);
        }
        for (int i = 3; i <= 5; i++)
        {
            isAvailable[texts[i]] = GameManager.Instance.Level > 2;
            arrows[i].color = new Color(1, 1, 1, 0f);
            UpdateTextAndIcon(i);
        }
    }

    private void UpdateTextAndIcon(int index)
    {
        var text = texts[index];
        icons[index].color = new Color(1, 1, 1, 1);

        if (!isAvailable.ContainsKey(text) || !isAvailable[text])
        {
            text.color = new Color(1, 1, 1, 0.3f);
            icons[index].sprite = locked;
        }
        else if (isSelected[text])
        {
            text.color = new Color(1, 1, 1, 0.3f);
            icons[index].sprite = selected;
            if (text == characterSelected)
                arrows[index].color = new Color(1, 1, 1, 1f);
        }
        else
        {
            text.color = Color.white;
            icons[index].sprite = null;
            icons[index].color = new Color(1, 1, 1, 0f);
        }
    }

    private int FindCharacterSelected(TextMeshProUGUI name)
    {
        for (int i = 0; i < texts.Length; i++)
            if (texts[i] == name)
                return i;

        return -1;
    }

    public void PlayerSelected(string name, string previousPG)
    {
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].text == previousPG)
                isSelected[texts[i]] = false;

            if (texts[i].text == name)
            {
                isSelected[texts[i]] = true;
            }
        }
        UpdateUI();
    }
    #endregion
    #region Player Selection
    private void SelectPG(int index)
    {
        TextMeshProUGUI pg = texts[index];
        int previousIndex = -1;

        if (!isAvailable[pg] || characterSelected == pg || isSelected[pg])
            return;

        if (characterSelected == null)
        {
            GameManager.Instance.AddPlayer();
        }
        else
        {
            previousIndex = FindCharacterSelected(characterSelected);
            isSelected[characterSelected] = false;
            arrows[previousIndex].color = new Color(1, 1, 1, 0f);
            UpdateTextAndIcon(previousIndex);
        }

        characterSelected = pg;
        isSelected[characterSelected] = true;
        UpdateTextAndIcon(index);
        room.SetPlayer(characterSelected.text, previousIndex == -1 ? "" : texts[previousIndex].text);
        UpdateUI();
    }
    #endregion
}
