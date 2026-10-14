using UnityEditor;
using UnityEngine;

/// <summary>
/// Кастомный инспектор для GridManager.
/// Показывает параметры выбранного алгоритма.
/// </summary>
[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    private SerializedProperty _algorithmProp;
    private SerializedProperty _loopChanceProp;

    // BSP
    private SerializedProperty _bspMinRoomSizeProp;
    private SerializedProperty _bspMaxRoomSizeProp;
    private SerializedProperty _bspMinRoomsProp;
    private SerializedProperty _bspMaxRoomsProp;

    // CA
    private SerializedProperty _caFillChanceProp;
    private SerializedProperty _caIterationsProp;
    private SerializedProperty _caMinRegionSizeProp;

    private void OnEnable()
    {
        _algorithmProp = serializedObject.FindProperty("_algorithm");
        _loopChanceProp = serializedObject.FindProperty("_loopChance");

        _bspMinRoomSizeProp = serializedObject.FindProperty("_bspMinRoomSize");
        _bspMaxRoomSizeProp = serializedObject.FindProperty("_bspMaxRoomSize");
        _bspMinRoomsProp = serializedObject.FindProperty("_bspMinRooms");
        _bspMaxRoomsProp = serializedObject.FindProperty("_bspMaxRooms");

        _caFillChanceProp = serializedObject.FindProperty("_caFillChance");
        _caIterationsProp = serializedObject.FindProperty("_caIterations");
        _caMinRegionSizeProp = serializedObject.FindProperty("_caMinRegionSize");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Рисуем всё, кроме условных полей
        DrawPropertiesExcluding(serializedObject,
            "_bspMinRoomSize", "_bspMaxRoomSize", "_bspMinRooms", "_bspMaxRooms",
            "_caFillChance", "_caIterations", "_caMinRegionSize");

        var algorithm = (GenerationAlgorithm)_algorithmProp.enumValueIndex;

        // BSP-параметры
        if (algorithm == GenerationAlgorithm.BSP)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("BSP — параметры Кладбища", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_bspMinRoomSizeProp);
            EditorGUILayout.PropertyField(_bspMaxRoomSizeProp);
            EditorGUILayout.PropertyField(_bspMinRoomsProp);
            EditorGUILayout.PropertyField(_bspMaxRoomsProp);
            EditorGUI.indentLevel--;

            if (_bspMinRoomSizeProp.intValue > _bspMaxRoomSizeProp.intValue)
            {
                EditorGUILayout.HelpBox("Min Room Size больше, чем Max Room Size!", MessageType.Warning);
            }

            if (_bspMinRoomsProp.intValue > _bspMaxRoomsProp.intValue)
            {
                EditorGUILayout.HelpBox("Min Rooms больше, чем Max Rooms!", MessageType.Warning);
            }
        }

        // CA-параметры
        if (algorithm == GenerationAlgorithm.CellularAutomata)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("CA — параметры Пепельного леса", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_caFillChanceProp);
            EditorGUILayout.PropertyField(_caIterationsProp);
            EditorGUILayout.PropertyField(_caMinRegionSizeProp);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
