using UnityEngine;
using UnityEditor;
using System.IO;
using NUnit.Framework;

[CustomEditor(typeof(Biomes))]
public class BiomeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Biomes biomes = (Biomes)target;
        if (GUILayout.Button("Serialize to JSON"))
        {
            string json = biomes.SerializeToJson();
            string folderPath = Path.Combine(Application.dataPath, "JSON");
            string filePath = Path.Combine(folderPath, "biomes.json");

            // Create the JSON folder if it doesn't exist
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            File.WriteAllText(filePath, json);

            Debug.Log($"JSON file saved to: {filePath}");
        }

        if (GUILayout.Button("Load from JSON"))
        {
            if (biomes.JsonFile.text.Equals(string.Empty))
                throw new InvalidDataException("Cannot load empty json file");
            biomes.AllBiomes = JsonUtility.FromJson<Biomes.IndexValueList>(biomes.JsonFile.text);
        }
    }
}
