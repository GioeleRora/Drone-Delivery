using UnityEngine;

public class ProceduralMapBuilder : MonoBehaviour
{
    [Header("Impostazioni Mappa")]
    public int gridSize = 60;
    public float spacing = 10f;
    public float maxElevation = 15f;
    public float biomeScale = 0.006f;
    public float elevationScale = 0.015f;

    [Header("Prefabs Cubi Colorati")]
    public GameObject cityBuildingPrefab;
    public GameObject ruinPrefab;
    public GameObject parkTreePrefab;
    public GameObject roadPrefab;

    [ContextMenu("Genera Mappa Procedurale")]
    public void WorldMap()
    {
        // Svuota la vecchia mappa se presente
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Cerca e pulisce il vecchio terreno se esiste già nella scena
        GameObject oldTerrain = GameObject.Find("ProceduralTerrain");
        if (oldTerrain != null) DestroyImmediate(oldTerrain);

        // Configurazione dati Terreno
        TerrainData td = new TerrainData();
        td.heightmapResolution = gridSize + 1;
        td.size = new Vector3(gridSize * spacing, maxElevation, gridSize * spacing);

        float[,] heights = new float[gridSize + 1, gridSize + 1];
        float offset = (gridSize - 1) * spacing / 2f;

        // Calcolo altezze del terreno
        for (int x = 0; x <= gridSize; x++)
        {
            for (int z = 0; z <= gridSize; z++)
            {
                float wx = x * spacing - offset;
                float wz = z * spacing - offset;
                
                float bNoise = Mathf.PerlinNoise(wx * biomeScale + 200f, wz * biomeScale + 200f);
                float hillMult = Mathf.Clamp01((0.25f - bNoise) / 0.15f);
                
                heights[z, x] = Mathf.PerlinNoise(wx * elevationScale, wz * elevationScale) * hillMult;
            }
        }
        td.SetHeights(0, 0, heights);

        // Creazione fisica del terreno nella scena
        GameObject terrainObj = Terrain.CreateTerrainGameObject(td);
        terrainObj.name = "ProceduralTerrain";
        terrainObj.transform.position = new Vector3(-offset, 0, -offset);

        // Ciclo di posizionamento dei cubi colorati
        float depth = 5f;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                float wx = x * spacing - offset;
                float wz = z * spacing - offset;

                float bNoise = Mathf.PerlinNoise(wx * biomeScale + 200f, wz * biomeScale + 200f);
                float rNoise = Mathf.PerlinNoise(wx * 0.05f + 500f, wz * 0.05f + 500f);
                
                float y = terrainObj.GetComponent<Terrain>().SampleHeight(new Vector3(wx, 0, wz));

                GameObject prefabToSpawn = null;
                Vector3 scale = Vector3.one;
                float finalY = y;

                // Area di spawn sicura al centro
                if (x == gridSize / 2 && z == gridSize / 2)
                {
                    prefabToSpawn = roadPrefab;
                    scale = new Vector3(10f, 0.2f, 10f);
                }
                else if (rNoise >= 0.48f && rNoise <= 0.52f)
                {
                    prefabToSpawn = roadPrefab;
                    scale = new Vector3(10f, 0.2f, 10f);
                }
                else if (bNoise > 0.52f)
                {
                    prefabToSpawn = cityBuildingPrefab;
                    float scalaY = Random.Range(10f, 40f) + depth;
                    scale = new Vector3(7f, scalaY, 7f);
                    finalY = y + (scalaY / 2f) - depth;
                }
                else if (bNoise > 0.25f)
                {
                    prefabToSpawn = ruinPrefab;
                    float scalaY = Random.Range(2f, 5f) + depth;
                    scale = new Vector3(7f, scalaY, 7f);
                    finalY = y + (scalaY / 2f) - depth;
                }
                else
                {
                    prefabToSpawn = parkTreePrefab;
                    float scalaY = Random.Range(4f, 7f) + depth;
                    scale = new Vector3(2f, scalaY, 2f);
                    finalY = y + (scalaY / 2f) - depth;
                }

                if (prefabToSpawn != null)
                {
                    GameObject instance = Instantiate(prefabToSpawn, new Vector3(wx, finalY, wz), Quaternion.identity, transform);
                    instance.transform.localScale = scale;
                }
            }
        }
    }
}
