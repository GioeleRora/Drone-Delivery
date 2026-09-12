using UnityEngine;
using UnityEditor;

public class VoxelArchitect : EditorWindow
{
    [MenuItem("Tools/Genera Voxel Cratere (Nuovo Motore)")]
    public static void GenerateVoxelCrater()
    {
        GameObject existing = GameObject.Find("VoxelCraterContainer");
        if (existing != null) DestroyImmediate(existing);

        ComputeShader shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/Environment/MarchingCubes.compute");
        if (shader == null) return;

        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Mat_CyberpunkCity.mat");

        GameObject container = new GameObject("VoxelCraterContainer");

        int chunkSize = 32;
        float spacing = 5f; // 5 metri per voxel. Chunk fisico: 155 metri.
        float chunkPhysicalSize = (chunkSize - 1) * spacing; 

        // Raggio 800m -> Diametro 1600m (Allargato per permettere tunnel più lunghi!)
        int chunksX = Mathf.CeilToInt(1600f / chunkPhysicalSize);
        int chunksZ = Mathf.CeilToInt(1600f / chunkPhysicalSize);
        // Altezza da -450 a 550 (Totale 1000m) per dare margine di sicurezza sotto il pavimento
        int chunksY = Mathf.CeilToInt(1000f / chunkPhysicalSize);

        float startX = -800f;
        float startY = -450f;
        float startZ = -800f;

        int chunksGenerated = 0;

        for (int cx = 0; cx < chunksX; cx++)
        {
            for (int cz = 0; cz < chunksZ; cz++)
            {
                // CULLING 2D: Troviamo min e max altezza in questo blocco
                float minH = 9999f;
                float maxH = -9999f;
                float wX = startX + cx * chunkPhysicalSize;
                float wZ = startZ + cz * chunkPhysicalSize;

                for (int sx = 0; sx <= 2; sx++)
                {
                    for (int sz = 0; sz <= 2; sz++)
                    {
                        float h = CraterMath.GetSurfaceHeight(wX + (sx/2f)*chunkPhysicalSize, wZ + (sz/2f)*chunkPhysicalSize);
                        if (h < minH) minH = h;
                        if (h > maxH) maxH = h;
                    }
                }

                minH -= 60f; // Margine di pendenza
                maxH += 60f;

                // Non cullingare la zona dell'Isola Zenit (verifichiamo l'intersezione del chunk)
                bool isIslandZone = (wX + chunkPhysicalSize > -250f && wX < 250f && wZ + chunkPhysicalSize > -250f && wZ < 250f);

                for (int cy = 0; cy < chunksY; cy++)
                {
                    float wY = startY + cy * chunkPhysicalSize;

                    // CULLING 3D: Saltiamo se siamo in aria o profondamente sottoterra
                    // Se siamo nella zona dell'isola tra i 100m e i 450m, saltiamo il culling
                    bool isIslandHeight = isIslandZone && (wY + chunkPhysicalSize > 100f && wY < 450f);
                    
                    // Controlliamo se questo chunk interseca un tunnel. Se sì, NON dobbiamo cullingarlo
                    // anche se è profondamente sottoterra, altrimenti il tunnel avrà i muri bucati!
                    Vector3 chunkCenter = new Vector3(wX + chunkPhysicalSize/2f, wY + chunkPhysicalSize/2f, wZ + chunkPhysicalSize/2f);
                    bool isTunnelChunk = CraterMath.IntersectsTunnel(chunkCenter, chunkPhysicalSize);

                    if (!isIslandHeight && !isTunnelChunk)
                    {
                        if (wY > maxH) continue;
                        if (wY + chunkPhysicalSize < minH) continue;
                    }

                    GameObject chunkObj = new GameObject($"Chunk_{cx}_{cy}_{cz}");
                    chunkObj.transform.SetParent(container.transform);
                    chunkObj.transform.position = new Vector3(wX + chunkPhysicalSize/2f, wY + chunkPhysicalSize/2f, wZ + chunkPhysicalSize/2f);

                    VoxelChunk chunk = chunkObj.AddComponent<VoxelChunk>();
                    chunk.marchingCubesShader = shader;
                    chunk.numPointsPerAxis = chunkSize;
                    chunk.boundsSize = chunkPhysicalSize;
                    chunk.isoLevel = 0f;

                    if (mat != null) chunkObj.GetComponent<MeshRenderer>().sharedMaterial = mat;

                    float[] density = new float[chunkSize * chunkSize * chunkSize];

                    for (int z = 0; z < chunkSize; z++)
                    {
                        for (int y = 0; y < chunkSize; y++)
                        {
                            for (int x = 0; x < chunkSize; x++)
                            {
                                int idx = x + chunkSize * (y + chunkSize * z);
                                density[idx] = CraterMath.GetDensity(wX + x * spacing, wY + y * spacing, wZ + z * spacing);
                            }
                        }
                    }

                    chunk.GenerateMesh(density);
                    chunksGenerated++;
                }
            }
        }
        
        Debug.Log($"🚀 [VoxelEngine] Cratere completo generato in Voxel! Chunks: {chunksGenerated}");
    }
}
