using UnityEditor;
using UnityEngine;
using System.Collections;


[CustomEditor(typeof(HexGridComp))]
[CanEditMultipleObjects]
public class HexGridEditor : Editor
{
  SerializedProperty m_gridWidthProp;
  SerializedProperty m_gridHeightProp;
  SerializedProperty m_horizontalTileDistProp;
  SerializedProperty m_verticalTileDistProp;
  SerializedProperty m_tileOrientationProp;

  SerializedProperty m_pegPrefabProp;
  SerializedProperty m_hexPrefabProp;

  private void OnEnable()
  {
    m_gridWidthProp = serializedObject.FindProperty("m_gridWidth");
    m_gridHeightProp = serializedObject.FindProperty("m_gridHeight");
    m_horizontalTileDistProp = serializedObject.FindProperty("m_horizontalTileDist");
    m_verticalTileDistProp = serializedObject.FindProperty("m_verticalTileDist");
    m_tileOrientationProp = serializedObject.FindProperty("m_tileOrientation");
    m_pegPrefabProp = serializedObject.FindProperty("m_pegPrefab");
    m_hexPrefabProp = serializedObject.FindProperty("m_hexPrefab");
  }
  public override void OnInspectorGUI()
  {/*
    serializedObject.Update();

    EditorGUILayout.PropertyField(m_gridWidthProp,)


    serializedObject.ApplyModifiedProperties();
  */
    DrawDefaultInspector();

    HexGridComp rHexGrid = (HexGridComp)target;
    if(GUILayout.Button("Generate Hex Grid"))
    {
      rHexGrid.Generate();
      rHexGrid.GenerateNavGraph();
    }
  }

}
