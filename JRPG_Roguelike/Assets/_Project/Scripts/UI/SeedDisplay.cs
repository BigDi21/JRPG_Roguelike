using TMPro;
using UnityEngine;

/// <summary>
/// Показывает текущий seed на экране. Для отладки.
/// </summary>
public class SeedDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text _seedText;

    private void Start() => UpdateSeedText();

    private void Update() =>
        // Обновляем каждый кадр, пока не сгенерируется сетка.
        UpdateSeedText();

    private void UpdateSeedText()
    {
        if (_seedText == null)
        {
            return;
        }

        if (GridManager.Instance == null)
        {
            return;
        }

        _seedText.text = $"Seed: {GridManager.Instance.CurrentSeed}";
    }
}
