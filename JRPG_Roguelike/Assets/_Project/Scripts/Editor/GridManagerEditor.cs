using UnityEditor;
using UnityEngine;

/// <summary>
/// Кастомный инспектор для GridManager.
/// Показывает BSP-параметры только когда выбран алгоритм BSP.
/// </summary>
[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    private SerializedProperty _algorithmProp;
    private SerializedProperty _bspMinRoomSizeProp;
    private SerializedProperty _bspMaxRoomSizeProp;
    private SerializedProperty _bspMinRoomsProp;
    private SerializedProperty _bspMaxRoomsProp;

    private void OnEnable()
    {
        _algorithmProp = serializedObject.FindProperty("_algorithm");
        _bspMinRoomSizeProp = serializedObject.FindProperty("_bspMinRoomSize");
        _bspMaxRoomSizeProp = serializedObject.FindProperty("_bspMaxRoomSize");
        _bspMinRoomsProp = serializedObject.FindProperty("_bspMinRooms");
        _bspMaxRoomsProp = serializedObject.FindProperty("_bspMaxRooms");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Рисуем всё, кроме BSP-полей
        DrawPropertiesExcluding(serializedObject,
            "_bspMinRoomSize",
            "_bspMaxRoomSize",
            "_bspMinRooms",
            "_bspMaxRooms");

        // Если выбран BSP — показываем его параметры
        if (_algorithmProp != null &&
            (GenerationAlgorithm)_algorithmProp.enumValueIndex == GenerationAlgorithm.BSP)
        {
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("BSP — параметры Кладбища", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_bspMinRoomSizeProp);
            EditorGUILayout.PropertyField(_bspMaxRoomSizeProp);
            EditorGUILayout.PropertyField(_bspMinRoomsProp);
            EditorGUILayout.PropertyField(_bspMaxRoomsProp);
            EditorGUI.indentLevel--;

            // Валидация
            if (_bspMinRoomSizeProp.intValue > _bspMaxRoomSizeProp.intValue)
            {
                EditorGUILayout.HelpBox(
                    "Min Room Size больше, чем Max Room Size!",
                    MessageType.Warning);
            }

            if (_bspMinRoomsProp.intValue > _bspMaxRoomsProp.intValue)
            {
                EditorGUILayout.HelpBox(
                    "Min Rooms больше, чем Max Rooms!",
                    MessageType.Warning);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
