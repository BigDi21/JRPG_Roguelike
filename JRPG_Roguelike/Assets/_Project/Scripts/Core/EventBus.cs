using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Статическая шина событий. Позволяет системам общаться без прямых ссылок.
/// Поддерживает события-структуры. Всегда отписывайтесь в OnDisable/OnDestroy.
/// </summary>
public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> _handlers = new();

    /// <summary>
    /// Подписывает обработчик на событие указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип события (структура).</typeparam>
    /// <param name="handler">Обработчик.</param>
    public static void Subscribe<T>(Action<T> handler) where T : struct
    {
        if (handler == null)
        {
            Debug.LogError("[EventBus] Попытка подписаться с null-обработчиком!");
            return;
        }

        Type type = typeof(T);

        _handlers[type] = _handlers.TryGetValue(type, out Delegate existing) ? Delegate.Combine(existing, handler) : handler;
    }

    /// <summary>
    /// Отписывает обработчик от события указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип события (структура).</typeparam>
    /// <param name="handler">Обработчик.</param>
    public static void Unsubscribe<T>(Action<T> handler) where T : struct
    {
        if (handler == null)
        {
            return;
        }

        Type type = typeof(T);

        if (!_handlers.TryGetValue(type, out Delegate existing))
        {
            return;
        }

        var result = Delegate.Remove(existing, handler);

        if (result == null)
        {
            _handlers.Remove(type);
        }
        else
        {
            _handlers[type] = result;
        }
    }

    /// <summary>
    /// Поднимает событие. Все подписчики будут уведомлены.
    /// </summary>
    /// <typeparam name="T">Тип события (структура).</typeparam>
    /// <param name="evt">Событие.</param>
    public static void Raise<T>(T evt) where T : struct
    {
        if (_handlers.TryGetValue(typeof(T), out Delegate handler))
        {
            (handler as Action<T>)?.Invoke(evt);
        }
    }

    /// <summary>
    /// Очищает все подписки. Используется при перезагрузке сцены.
    /// </summary>
    public static void Clear() => _handlers.Clear();
}
