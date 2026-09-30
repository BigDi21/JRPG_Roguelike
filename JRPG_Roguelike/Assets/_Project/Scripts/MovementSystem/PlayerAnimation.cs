using UnityEngine;

/// <summary>
/// Управляет анимациями игрока на основе состояния <see cref="PlayerMovement"/>.
/// </summary>
public class PlayerAnimation : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private PlayerMovement _movement;

    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();

        if (_animator == null)
            Debug.LogError($"[PlayerAnimation] На {gameObject.name} отсутствует компонент Animator!");
    }

    private void Update()
    {
        if (_movement == null || _animator == null) return;

        _animator.SetBool("isRune", _movement.IsMoving);
    }
}