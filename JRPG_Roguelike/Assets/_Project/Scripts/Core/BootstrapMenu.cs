using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Меню стартовой сцены Bootstrap. Позволяет перейти в любую из основных сцен.
/// </summary>
public class BootstrapMenu : MonoBehaviour
{
    [Header("Имена сцен")]
    [Tooltip("Имя сцены с хабом. Должно совпадать с Build Settings.")]
    [SerializeField] private string _hubSceneName = "Hub";

    [Tooltip("Имя боевой сцены. Должно совпадать с Build Settings.")]
    [SerializeField] private string _battleSceneName = "Battle";

    [Tooltip("Имя сцены с системой перемещения. Должно совпадать с Build Settings.")]
    [SerializeField] private string _movementSceneName = "Movement";

    [Header("Кнопки")]
    [SerializeField] private Button _hubButton;
    [SerializeField] private Button _battleButton;
    [SerializeField] private Button _movementButton;

    private void Start() => SubscribeButtons();

    private void OnDestroy() => UnsubscribeButtons();

    private void SubscribeButtons()
    {
        if (_hubButton != null)
        {
            _hubButton.onClick.AddListener(LoadHub);
        }

        if (_battleButton != null)
        {
            _battleButton.onClick.AddListener(LoadBattle);
        }

        if (_movementButton != null)
        {
            _movementButton.onClick.AddListener(LoadMovement);
        }
    }

    private void UnsubscribeButtons()
    {
        if (_hubButton != null)
        {
            _hubButton.onClick.RemoveListener(LoadHub);
        }

        if (_battleButton != null)
        {
            _battleButton.onClick.RemoveListener(LoadBattle);
        }

        if (_movementButton != null)
        {
            _movementButton.onClick.RemoveListener(LoadMovement);
        }
    }

    /// <summary>Загружает сцену Хаба.</summary>
    public void LoadHub() => LoadScene(_hubSceneName);

    /// <summary>Загружает боевую сцену.</summary>
    public void LoadBattle() => LoadScene(_battleSceneName);

    /// <summary>Загружает сцену с системой перемещения.</summary>
    public void LoadMovement() => LoadScene(_movementSceneName);

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"[BootstrapMenu] Имя сцены не задано!");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[BootstrapMenu] Сцена '{sceneName}' не найдена в Build Settings!");
            return;
        }

        Debug.Log($"[BootstrapMenu] Загрузка сцены: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
