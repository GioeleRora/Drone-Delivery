using UnityEngine;
using UnityEditor;
using System.IO;

public class MeshyMaterialSetup
{
    [MenuItem("Tools/Configura Materiali Meshy Automatici")]
    public static void SetupMaterial()
    {
        // Prende la cartella attualmente selezionata nella finestra Project
        string folderPath = GetSelectedPathOrFallback();
        
        // Trova i file png e fbx nella cartella
        string[] textureFiles = Directory.GetFiles(folderPath, "*.png");
        string[] fbxFiles = Directory.GetFiles(folderPath, "*.fbx");

        if (fbxFiles.Length == 0) {
            Debug.LogError("Nessun file .fbx trovato nella cartella selezionata: " + folderPath);
            return;
        }

        string fbxPath = fbxFiles[0].Replace("\\", "/");
        Texture2D albedo = null, normal = null, metallic = null;

        foreach (string file in textureFiles)
        {
            string path = file.Replace("\\", "/");
            if (path.Contains("_normal"))
            {
                normal = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                // Converte in Normal Map
                TextureImporter normalImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (normalImporter != null && normalImporter.textureType != TextureImporterType.NormalMap)
                {
                    normalImporter.textureType = TextureImporterType.NormalMap;
                    normalImporter.SaveAndReimport();
                }
            }
            else if (path.Contains("_metallic"))
            {
                metallic = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            else if (!path.Contains("roughness")) // Esclude altre mappe per ora
            {
                albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
        }

        if (albedo == null) {
            Debug.LogError("Non trovo la texture principale (Albedo) in " + folderPath);
            return;
        }

        string matName = "Mat_" + new DirectoryInfo(folderPath).Name + ".mat";
        string matPath = folderPath + "/" + matName;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        
        Material mat = new Material(shader);
        mat.SetTexture("_BaseMap", albedo);
        mat.SetTexture("_MainTex", albedo);
        
        if (normal != null) {
            mat.SetTexture("_BumpMap", normal);
            mat.EnableKeyword("_NORMALMAP");
        }
        
        if (metallic != null) {
            mat.SetTexture("_MetallicGlossMap", metallic);
            mat.SetFloat("_Smoothness", 0.4f);
        }

        AssetDatabase.CreateAsset(mat, matPath);
        
        ModelImporter fbxImporter = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (fbxImporter != null)
        {
            fbxImporter.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            fbxImporter.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnMaterialName, ModelImporterMaterialSearch.Everywhere);
            
            // Per semplicità usiamo il metodo di remap nativo
            fbxImporter.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "Material.001"), mat);
            fbxImporter.SaveAndReimport();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ SUCCESSO: Materiale generato e assegnato in " + folderPath);
    }

    private static string GetSelectedPathOrFallback()
    {
        string path = "Assets";
        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
            else if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                return path; // Se l'utente clicca direttamente la cartella
            }
        }
        return path;
    }
}
