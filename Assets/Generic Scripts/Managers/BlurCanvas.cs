using UnityEngine;

//Script per collegare il prefab del blur durante la pausa con il GameManager
public class BlurCanvas : MonoBehaviour
{
    [SerializeField] private GameObject blurEffectPrefab;

    void Start()
    {
        if (blurEffectPrefab != null)
            GameManager.Instance.SetBlur(blurEffectPrefab);
    }
}
