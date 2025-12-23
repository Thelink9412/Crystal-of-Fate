using UnityEngine;

[CreateAssetMenu(fileName = "Player", menuName = "Player/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public float maxHealth;
    public float attack;
    public string playerName;
    public float atkCd;
    public float abilityCd;
}