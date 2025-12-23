using UnityEngine;
using UnityEngine.UI;

//Script per gestire la barra della vita
public class Healthbar : MonoBehaviour
{
    [SerializeField] private Image _foreground;
    public void UpdateHealthbar(float maxHealt, float currentHealth)
    {
        _foreground.fillAmount = currentHealth / maxHealt;
    }
}
