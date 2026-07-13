using UnityEngine;
using UnityEditor;

public class ZenithIslandArchitect : EditorWindow
{
    [MenuItem("Tools/Genera Isola Zenit")]
    public static void GenerateZenith()
    {
        GameObject existing = GameObject.Find("ZenithIsland");
        if (existing != null)
        {
            DestroyImmediate(existing);
        }

        GameObject zenith = new GameObject("ZenithIsland");
        zenith.isStatic = true;
        // Posizioniamo l'isola in alto sopra il centro del cratere
        zenith.transform.position = new Vector3(0, 400f, 0); 

        MeshFilter mf = zenith.AddComponent<MeshFilter>();
        MeshRenderer mr = zenith.AddComponent<MeshRenderer>();
        MeshCollider mc = zenith.AddComponent<MeshCollider>();

        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Mat_CyberpunkCity.mat");
        if (mat == null) mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
        mr.sharedMaterial = mat;

        Mesh mesh = CreateIslandMesh(120f, 200f, 40, 80); // Raggio 120m, Profondità 200m, 40 anelli (sotto), 80 segmenti
        mesh.name = "ZenithProceduralMesh";
        
        mf.sharedMesh = mesh;
        mc.sharedMesh = mesh;

        Debug.Log("☁️ [ZenithIslandArchitect] Isola Zenit generata con successo!");
    }

    private static Mesh CreateIslandMesh(float maxRadius, float maxDepth, int rings, int segments)
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        // Vertici totali:
        // Top surface: ringsTop * segments + 1 (centro)
        // Bottom surface: ringsBottom * segments + 1 (punta inferiore)
        // Per semplificare, usiamo 10 anelli per il top piatto e 'rings' per il bottom roccioso.
        int topRings = 10;
        int bottomRings = rings;
        int totalVertices = (topRings * segments + 1) + (bottomRings * segments + 1);
        
        Vector3[] vertices = new Vector3[totalVertices];
        Vector2[] uvs = new Vector2[totalVertices];

        // 1. Generiamo il perimetro base (distorto dal noise) in modo che top e bottom combacino perfettamente
        float[] perimeterRadii = new float[segments];
        for (int s = 0; s < segments; s++)
        {
            float angle = s * Mathf.PI * 2f / segments;
            float nx = Mathf.Cos(angle);
            float nz = Mathf.Sin(angle);
            
            // Perlin noise per il perimetro irregolare
            float noise = (Mathf.PerlinNoise(nx * 1.2f + 50f, nz * 1.2f + 50f) - 0.5f) * (maxRadius * 0.6f);
            perimeterRadii[s] = maxRadius + noise;
        }

        int vIdx = 0;

        // --- TOP SURFACE (Piatta ma frastagliata sul bordo) ---
        int topCenterIdx = vIdx;
        vertices[vIdx] = new Vector3(0, 0, 0); // Centro top
        uvs[vIdx] = new Vector2(0, 0);
        vIdx++;

        for (int r = 1; r <= topRings; r++)
        {
            float rPercent = (float)r / topRings;
            for (int s = 0; s < segments; s++)
            {
                float angle = s * Mathf.PI * 2f / segments;
                float currentRadius = perimeterRadii[s] * rPercent;
                
                float posX = Mathf.Cos(angle) * currentRadius;
                float posZ = Mathf.Sin(angle) * currentRadius;
                float posY = 0f; // Superficie completamente piatta per poterci costruire sopra

                vertices[vIdx] = new Vector3(posX, posY, posZ);
                uvs[vIdx] = new Vector2(posX / 10f, posZ / 10f);
                vIdx++;
            }
        }
        int topEndIdx = vIdx - 1;

        // --- BOTTOM SURFACE (Rocciosa, imprevedibile, a punte multiple) ---
        int bottomCenterIdx = vIdx;
        float centerPosY = GetBottomY(0, 0, 0, maxDepth);
        vertices[vIdx] = new Vector3(0, centerPosY, 0); 
        uvs[vIdx] = new Vector2(0, 0);
        vIdx++;

        for (int r = 1; r <= bottomRings; r++)
        {
            float rPercent = (float)r / bottomRings; 
            
            for (int s = 0; s < segments; s++)
            {
                float angle = s * Mathf.PI * 2f / segments;
                float nx = Mathf.Cos(angle);
                float nz = Mathf.Sin(angle);

                // La forma base è a scodella, non più a cono appuntito
                float profile = Mathf.Pow(rPercent, 0.7f); 
                float baseRadius = perimeterRadii[s] * profile;
                
                float px = nx * baseRadius;
                float pz = nz * baseRadius;
                
                float basePosY = GetBottomY(px, pz, rPercent, maxDepth);
                
                // Aggiungiamo rumore radiale per spezzare gli anelli
                float rimFade = Mathf.SmoothStep(1f, 0f, Mathf.Pow(rPercent, 3f));
                float rockNoiseR = (Mathf.PerlinNoise(px * 0.08f + 300.5f, pz * 0.08f + 300.5f) - 0.5f) * 20f;
                float currentRadius = baseRadius + (rockNoiseR * rimFade);
                
                // Evitiamo auto-intersezioni estreme
                currentRadius = Mathf.Max(currentRadius, 0.1f);

                float posX = nx * currentRadius;
                float posZ = nz * currentRadius;

                // Forza l'ultimo anello a combaciare ESATTAMENTE con il perimetro del Top
                if (r == bottomRings)
                {
                    posX = nx * perimeterRadii[s];
                    posZ = nz * perimeterRadii[s];
                    basePosY = 0f;
                }

                vertices[vIdx] = new Vector3(posX, basePosY, posZ);
                // Mappatura UV cilindrica per le rocce
                uvs[vIdx] = new Vector2(angle / (Mathf.PI * 2f) * 10f, basePosY / 10f); 
                vIdx++;
            }
        }

        // --- TRIANGOLI ---
        int topTriangles = (segments * 3) + ((topRings - 1) * segments * 6);
        int bottomTriangles = (segments * 3) + ((bottomRings - 1) * segments * 6);
        int[] triangles = new int[topTriangles + bottomTriangles];
        int tIdx = 0;

        // Top Surface (Winding order corretto per farla puntare verso l'ALTO)
        for (int s = 0; s < segments; s++)
        {
            triangles[tIdx++] = topCenterIdx;
            triangles[tIdx++] = topCenterIdx + 1 + (s + 1) % segments;
            triangles[tIdx++] = topCenterIdx + 1 + s;
        }

        for (int r = 0; r < topRings - 1; r++)
        {
            int ringStart = topCenterIdx + 1 + r * segments;
            int nextRingStart = ringStart + segments;
            for (int s = 0; s < segments; s++)
            {
                int current = ringStart + s;
                int next = ringStart + (s + 1) % segments;
                int currentUp = nextRingStart + s;
                int nextUp = nextRingStart + (s + 1) % segments;

                triangles[tIdx++] = current;
                triangles[tIdx++] = next;
                triangles[tIdx++] = currentUp;

                triangles[tIdx++] = next;
                triangles[tIdx++] = nextUp;
                triangles[tIdx++] = currentUp;
            }
        }

        // Bottom Surface (Winding order corretto per farla puntare verso il BASSO)
        for (int s = 0; s < segments; s++)
        {
            triangles[tIdx++] = bottomCenterIdx;
            triangles[tIdx++] = bottomCenterIdx + 1 + s;
            triangles[tIdx++] = bottomCenterIdx + 1 + (s + 1) % segments;
        }

        for (int r = 0; r < bottomRings - 1; r++)
        {
            int ringStart = bottomCenterIdx + 1 + r * segments;
            int nextRingStart = ringStart + segments;
            for (int s = 0; s < segments; s++)
            {
                int current = ringStart + s;
                int next = ringStart + (s + 1) % segments;
                int currentUp = nextRingStart + s;
                int nextUp = nextRingStart + (s + 1) % segments;

                triangles[tIdx++] = current;
                triangles[tIdx++] = currentUp;
                triangles[tIdx++] = next;

                triangles[tIdx++] = next;
                triangles[tIdx++] = currentUp;
                triangles[tIdx++] = nextUp;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private static float GetBottomY(float px, float pz, float rPercent, float maxDepth)
    {
        // 1. Forma globale: Unico grosso cono centrale
        // Un profilo a 0.5f crea una curva convessa "piena" che scende verso un'unica punta
        float profile = Mathf.Pow(rPercent, 0.5f);
        // Il cono principale scende fino all'80% della profondità massima
        float basePosY = -maxDepth * 0.8f * (1f - profile); 
        
        // 2. Rumore Macro (Punte Extra) 
        // Genera escrescenze secondarie che "escono" dal cono principale
        float stalactiteNoise = Mathf.PerlinNoise(px * 0.025f + 100.5f, pz * 0.025f + 100.5f);
        stalactiteNoise = Mathf.Pow(stalactiteNoise, 2f); // Rende le punte extra ben definite
        
        // Dissolvenza verso i bordi alti per combaciare con la superficie piana
        float rimFade = Mathf.SmoothStep(1f, 0f, Mathf.Pow(rPercent, 3f));
        
        // Spingiamo la roccia verso il basso nei punti di rumore alto (fino al 30% di profondità extra)
        float extraPointDrop = stalactiteNoise * (maxDepth * 0.3f) * rimFade;
        basePosY -= extraPointDrop;
        
        // 3. Rumore Micro (Dettaglio Roccioso Superficiale)
        float rockNoiseY = (Mathf.PerlinNoise(px * 0.07f + 200.5f, pz * 0.07f + 200.5f) - 0.5f) * 20f;
        basePosY += rockNoiseY * rimFade;
        
        return basePosY;
    }
}
