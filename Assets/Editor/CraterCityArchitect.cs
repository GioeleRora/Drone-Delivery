using UnityEngine;
using UnityEditor;

public class CraterCityArchitect : EditorWindow
{
    [MenuItem("Tools/Genera Cratere Base")]
    public static void GenerateCrater()
    {
        // Rimuove eventuali versioni precedenti per rapida iterazione
        GameObject existing = GameObject.Find("CraterCityBase");
        if (existing != null)
        {
            DestroyImmediate(existing);
        }

        // Crea il nuovo GameObject
        GameObject craterBase = new GameObject("CraterCityBase");
        craterBase.isStatic = true; // Ottimizzazione: è un terreno statico

        MeshFilter mf = craterBase.AddComponent<MeshFilter>();
        MeshRenderer mr = craterBase.AddComponent<MeshRenderer>();
        MeshCollider mc = craterBase.AddComponent<MeshCollider>();

        // Applica il materiale Cyberpunk o il fallback di base
        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Mat_CyberpunkCity.mat");
        if (mat == null)
        {
            Debug.LogWarning("Materiale Mat_CyberpunkCity non trovato. Uso il default material.");
            mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
        }
        mr.sharedMaterial = mat;

        // Genera la Mesh Procedurale (Risoluzione maggiorata, Raggio espanso a 600m)
        Mesh mesh = CreateCraterMesh(150, 600f);
        mesh.name = "CraterProceduralMesh";
        
        mf.sharedMesh = mesh;
        mc.sharedMesh = mesh;

        Debug.Log("🏗️ [CraterCityArchitect] CraterCityBase generato con successo!");
    }

    private static Mesh CreateCraterMesh(int resolution, float maxRadius)
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
        int rings = resolution;
        int segments = resolution * 2; // Doppia risoluzione angolare per cerchi morbidi
        
        int numVertices = rings * segments + 1;
        Vector3[] vertices = new Vector3[numVertices];
        Vector2[] uvs = new Vector2[numVertices];
        
        // Vertice centrale (Centro del Downtown)
        vertices[0] = new Vector3(0, -400f, 0);
        uvs[0] = new Vector2(0, 0);
        
        int vIdx = 1;
        for (int r = 1; r <= rings; r++)
        {
            float ringPercent = (float)r / rings;
            for (int s = 0; s < segments; s++)
            {
                float angle = s * Mathf.PI * 2f / segments;
                float nx = Mathf.Cos(angle);
                float nz = Mathf.Sin(angle);

                // Rumore di Perlin applicato radialmente (Ampiezze aumentate proporzionalmente alla scala 1.5x)
                float noiseOuter = (Mathf.PerlinNoise(nx * 1.5f + 10f, nz * 1.5f + 10f) - 0.5f) * 225f;
                float noiseMid = (Mathf.PerlinNoise(nx * 2.5f + 20f, nz * 2.5f + 20f) - 0.5f) * 150f;
                float noiseInner = (Mathf.PerlinNoise(nx * 1.0f + 30f, nz * 1.0f + 30f) - 0.5f) * 120f;

                float outerBound = maxRadius + noiseOuter; // Bordo esterno bianco
                float midBound = (maxRadius * 0.7f) + noiseMid; // Inizio discesa verde (allargato per pendenza meno scoscesa)
                float innerBound = (maxRadius * 0.15f) + noiseInner; // Downtown rosso (rimpicciolito)

                // Clamp per sicurezza (evita che i confini si intersechino)
                innerBound = Mathf.Clamp(innerBound, 10f, midBound - 20f);
                midBound = Mathf.Clamp(midBound, innerBound + 20f, outerBound - 20f);

                float currentRadius = ringPercent * outerBound;

                float posY = 0f;
                if (currentRadius <= innerBound)
                {
                    posY = -400f; // Downtown (Rosso)
                }
                else if (currentRadius <= midBound)
                {
                    // Pareti del cratere (Verde)
                    float t = (currentRadius - innerBound) / (midBound - innerBound);
                    
                    // 1. Distorsione: Primi due livelli in alto (spirale) molto più larghi
                    // Gli ultimi due in basso (concentrici) molto più stretti e ripidi.
                    float tDistorted;
                    if (t < 0.25f) {
                        tDistorted = (t / 0.25f) * 0.5f; // Il 25% del raggio copre il 50% dell'altezza (Ripido)
                    } else {
                        tDistorted = 0.5f + ((t - 0.25f) / 0.75f) * 0.5f; // Il 75% del raggio copre il 50% dell'altezza (Largo)
                    }
                    
                    // 2. Modello Concentrico (per il fondo)
                    float stepC = Mathf.Floor(tDistorted * 4f);
                    float fracC = (tDistorted * 4f) - stepC;
                    float terraceTC = fracC < 0.3f ? 0f : (fracC - 0.3f) / 0.7f; // Scarpate molto lunghe e ripide
                    terraceTC = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(terraceTC));
                    float finalTC = (stepC + terraceTC) / 4f;
                    finalTC = Mathf.Lerp(tDistorted, finalTC, 0.85f);
                    
                    // 3. Modello Spirale (per la cima)
                    float spiralOffset = (angle / (Mathf.PI * 2f)) * 0.25f;
                    float tSpiral = tDistorted + spiralOffset;
                    float stepS = Mathf.Floor(tSpiral * 4f);
                    float fracS = (tSpiral * 4f) - stepS;
                    float terraceTS = fracS < 0.6f ? 0f : (fracS - 0.6f) / 0.4f; // Molto spazio piatto (rampa larga)
                    terraceTS = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(terraceTS));
                    float finalTS = (stepS + terraceTS) / 4f - spiralOffset;
                    finalTS = Mathf.Lerp(tDistorted, finalTS, 0.85f);
                    
                    // 4. Fusione (Blend) tra Spirale in alto e Concentrico in basso
                    float blend = Mathf.SmoothStep(0.35f, 0.65f, tDistorted);
                    float finalT = Mathf.Lerp(finalTC, finalTS, blend);

                    // 5. Raccordo (Fade) ai bordi per evitare dirupi con il Parco e il Downtown
                    float edgeFade = Mathf.SmoothStep(0f, 1f, t * 15f) * Mathf.SmoothStep(0f, 1f, (1f - t) * 15f);
                    finalT = Mathf.Lerp(tDistorted, finalT, edgeFade);

                    posY = Mathf.Lerp(-400f, 150f, finalT);
                }
                else
                {
                    posY = 150f; // Residenziale/Parco (Bianco)
                }

                // Ondulamento del terreno:
                // Quasi azzerato all'interno del cratere (terrazzamenti e downtown) per costruire,
                // ridotto anche all'esterno (Parco).
                float surfaceNoiseAmp = (currentRadius > midBound) ? 2.5f : 0.5f;
                float surfaceNoise = (Mathf.PerlinNoise(nx * currentRadius * 0.05f, nz * currentRadius * 0.05f) - 0.5f) * surfaceNoiseAmp;
                posY += surfaceNoise;

                float posX = nx * currentRadius;
                float posZ = nz * currentRadius;

                vertices[vIdx] = new Vector3(posX, posY, posZ);
                uvs[vIdx] = new Vector2(posX / 10f, posZ / 10f); // Tiling a 10 metri
                vIdx++;
            }
        }

        int numTriangles = (segments * 3) + ((rings - 1) * segments * 6);
        int[] triangles = new int[numTriangles];
        int tIdx = 0;

        // Triangoli del cerchio centrale (Winding order orario corretto)
        for (int s = 0; s < segments; s++)
        {
            triangles[tIdx++] = 0;
            triangles[tIdx++] = 1 + (s + 1) % segments;
            triangles[tIdx++] = 1 + s;
        }

        // Triangoli degli anelli esterni (Winding order orario corretto)
        for (int r = 0; r < rings - 1; r++)
        {
            int ringStart = 1 + r * segments;
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

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
