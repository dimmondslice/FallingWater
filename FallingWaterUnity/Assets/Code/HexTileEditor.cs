using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(HexTileComp))]
[CanEditMultipleObjects]
public class HexTileEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    if (GUILayout.Button("Butt"))
    {

    }
  }
}
