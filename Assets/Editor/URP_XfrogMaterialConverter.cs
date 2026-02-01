using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class URP_XfrogMaterialConverter : EditorWindow
{
    private string folderPath = "Assets/Xfrog/2022 PBR XfrogPlants Sampler/Maps/Mat";

    [MenuItem("Tools/URP/Convert Xfrog Materials")]
    public static void ShowWindow()
    {
        GetWindow<URP_XfrogMaterialConverter>("URP Xfrog Material Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Convert Xfrog PBR Materials to URP Lit", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Convert Materials", GUILayout.Height(30)))
        {
            ConvertMaterials(folderPath);
        }
    }

    private static void ConvertMaterials(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError($"Folder not found: {folderPath}");
            return;
        }

        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { folderPath });
        int count = 0;

        foreach (string guid in materialGuids)
        {
            string matPath = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) continue;

            string baseName = Path.GetFileNameWithoutExtension(matPath);
            string folder = Path.GetDirectoryName(matPath);

            string[] allTextures = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                                            .Where(f => f.EndsWith(".png") || f.EndsWith(".jpg") || f.EndsWith(".tga"))
                                            .ToArray();

            mat.shader = Shader.Find("Universal Render Pipeline/Lit");

            Texture2D albedo = FindTexture(allTextures, baseName, "_D", "_Albedo", "_Color");
            Texture2D normal = FindTexture(allTextures, baseName, "_N", "_Normal");
            Texture2D mask = FindTexture(allTextures, baseName, "_RMA", "_Mask", "_MO", "_Metallic");

            if (albedo)
                mat.SetTexture("_BaseMap", albedo);

            if (normal)
            {
                mat.SetTexture("_BumpMap", normal);
                mat.EnableKeyword("_NORMALMAP");
            }

            if (mask)
                mat.SetTexture("_MaskMap", mask);

            mat.SetFloat("_Smoothness", 0.5f);
            mat.SetFloat("_Metallic", 0.1f);

            EditorUtility.SetDirty(mat);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Converted {count} Xfrog materials to URP Lit successfully!");
    }

    private static Texture2D FindTexture(string[] textures, string baseName, params string[] keywords)
    {
        foreach (string keyword in keywords)
        {
            string tex = textures.FirstOrDefault(t =>
                Path.GetFileNameWithoutExtension(t)
                    .ToLower().Contains(keyword.ToLower()) &&
                Path.GetFileNameWithoutExtension(t)
                    .ToLower().Contains(baseName.ToLower().Substring(0, Mathf.Min(6, baseName.Length)))
            );

            if (!string.IsNullOrEmpty(tex))
                return AssetDatabase.LoadAssetAtPath<Texture2D>(tex);
        }

        return null;
    }
}
