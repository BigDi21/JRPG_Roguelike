using UnityEngine;

/// <summary>
/// Глобальный менеджер игры. Загружает базу данных при старте и предоставляет к ней доступ.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("База данных")]
    [SerializeField] private GameDatabase _database;

    /// <summary>Центральный реестр всех ScriptableObject.</summary>
    public GameDatabase Database => _database;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        if (_database == null)
        {
            Debug.LogError("[GameManager] GameDatabase не назначена в инспекторе!");
            return;
        }

        _database.Initialize();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
