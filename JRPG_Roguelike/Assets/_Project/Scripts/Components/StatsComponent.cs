using UnityEngine;

public class StatsComponent : MonoBehaviour
{
    public int Strength = 10;
    public int Defense = 5;
    public int Magic = 8;
    public float Speed = 1.0f;

    [Header("Ресурсы")]
    public int MaxMana = 50;
    public int Mana = 50; // текущее значение

    public void UseMana(int amount) => Mana = Mathf.Max(0, Mana - amount);
    public void RestoreMana(int amount) => Mana = Mathf.Min(MaxMana, Mana + amount);
}