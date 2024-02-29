using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LayerTerrain))]
public class LayerTerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Serialize Params to JSON"))
        {
            LayerTerrain script = (LayerTerrain)target;
            script.SerializeNoiseParamsToJson();
        }
        if (GUILayout.Button("Load Params From JSON"))
        {
            LayerTerrain script = (LayerTerrain)target;
            script.LoadNoiseParamsFromJson();
        }
    }
}
