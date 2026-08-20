using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator _animator;
    [SerializeField]
    private PlayerMovement _movement;

    void Start()
    {
        _animator = GetComponent<Animator>();
        //_movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (_movement == null) return;

        // Используем флаг из PlayerMovement
        bool isRunning = _movement.IsMoving;
        _animator.SetBool("isRune", isRunning);
    }
}