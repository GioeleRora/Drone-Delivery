#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class GarageBuilder : MonoBehaviour
{
    [MenuItem("Drone Delivery/Genera Garage (Istanza Sotterranea)")]
    public static void BuildGarage()
    {
        // 1. Root del Garage
        GameObject garageRoot = GameObject.Find("GarageInstance");
        if (garageRoot != null)
        {
            DestroyImmediate(garageRoot);
        }
        
        garageRoot = new GameObject("GarageInstance");
        garageRoot.transform.position = new Vector3(0, -5000, 0);

        // Colori per il Greybox (Supporto URP)
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");

        Material wallMat = new Material(litShader);
        wallMat.color = new Color(0.2f, 0.2f, 0.2f); // Grigio scuro
        
        Material floorMat = new Material(litShader);
        floorMat.color = new Color(0.1f, 0.1f, 0.1f); // Quasi nero
        
        Material pedestalMat = new Material(litShader);
        pedestalMat.color = new Color(0.8f, 0.5f, 0.1f); // Arancione industriale
        
        Material doorMat = new Material(litShader);
        doorMat.color = new Color(0.3f, 0.4f, 0.5f); // Bluastro acciaio

        // 2. Pavimento
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(garageRoot.transform);
        floor.transform.localPosition = new Vector3(0, -0.5f, 0);
        floor.transform.localScale = new Vector3(30, 1, 30);
        floor.GetComponent<MeshRenderer>().sharedMaterial = floorMat;

        // 3. Muri
        CreateWall("Wall_North", new Vector3(0, 5, 15), new Vector3(30, 10, 1), wallMat, garageRoot.transform);
        CreateWall("Wall_South", new Vector3(0, 5, -15), new Vector3(30, 10, 1), wallMat, garageRoot.transform);
        CreateWall("Wall_East", new Vector3(15, 5, 0), new Vector3(1, 10, 30), wallMat, garageRoot.transform);
        // Muro Ovest avrà la saracinesca
        CreateWall("Wall_West_L", new Vector3(-15, 5, 10), new Vector3(1, 10, 10), wallMat, garageRoot.transform);
        CreateWall("Wall_West_R", new Vector3(-15, 5, -10), new Vector3(1, 10, 10), wallMat, garageRoot.transform);
        CreateWall("Wall_West_Top", new Vector3(-15, 8.5f, 0), new Vector3(1, 3, 10), wallMat, garageRoot.transform);

        // 4. Soffitto
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(garageRoot.transform);
        ceiling.transform.localPosition = new Vector3(0, 10.5f, 0);
        ceiling.transform.localScale = new Vector3(30, 1, 30);
        ceiling.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

        // 5. Saracinesca (Animabile)
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "GarageDoor";
        door.transform.SetParent(garageRoot.transform);
        door.transform.localPosition = new Vector3(-15, 3.5f, 0);
        door.transform.localScale = new Vector3(1, 7, 10);
        door.GetComponent<MeshRenderer>().sharedMaterial = doorMat;

        // 6. Piedistallo Centrale
        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.name = "DronePedestal";
        pedestal.transform.SetParent(garageRoot.transform);
        pedestal.transform.localPosition = new Vector3(0, 0.5f, 0);
        pedestal.transform.localScale = new Vector3(3, 1, 3);
        pedestal.GetComponent<MeshRenderer>().sharedMaterial = pedestalMat;

        // 7. Illuminazione (Stile Cyberpunk "Moody")
        GameObject spotlight = new GameObject("Spotlight");
        spotlight.transform.SetParent(garageRoot.transform);
        spotlight.transform.localPosition = new Vector3(0, 4.5f, 0); // 4 metri sopra il piedistallo
        spotlight.transform.localRotation = Quaternion.Euler(90, 0, 0); // Punta dritto in giù
        Light light = spotlight.AddComponent<Light>();
        light.type = LightType.Spot;
        light.spotAngle = 55;
        light.intensity = 50; // Intensità molto alta per contrastare il buio
        light.range = 10;
        light.color = new Color(0.8f, 0.9f, 1f); // Azzurrino freddo industriale
        light.shadows = LightShadows.Soft;
        
        // Rimuoviamo l'Ambient Fill per mantenere l'ambiente oscuro, o lo teniamo bassissimo
        GameObject ambientLight = new GameObject("AmbientFill");
        ambientLight.transform.SetParent(garageRoot.transform);
        ambientLight.transform.localPosition = new Vector3(0, 5, 0);
        Light fillLight = ambientLight.AddComponent<Light>();
        fillLight.type = LightType.Point;
        fillLight.intensity = 0.2f; // Quasi buio
        fillLight.range = 30;

        // 8. Setup della Telecamera Dedicata (Regia)
        GameObject cameraObj = new GameObject("GarageCamera");
        cameraObj.transform.SetParent(garageRoot.transform);
        Camera garageCam = cameraObj.AddComponent<Camera>();
        garageCam.fieldOfView = 40f; // FOV basso per far sembrare il drone massiccio
        
        // Creazione dei Target per l'HubCameraDirector
        GameObject targetsRoot = new GameObject("CameraTargets");
        targetsRoot.transform.SetParent(garageRoot.transform);
        targetsRoot.transform.localPosition = Vector3.zero;

        // Target Officina (Focus centrale)
        GameObject targetOfficina = new GameObject("OfficinaFocus");
        targetOfficina.transform.SetParent(targetsRoot.transform);
        targetOfficina.transform.localPosition = new Vector3(-8f, 6f, -10f); // Spostata di lato e sollevata
        targetOfficina.transform.localRotation = Quaternion.Euler(15f, 40f, 0f); // Guarda verso il piedistallo

        // Target Negozio (Spostata più indietro e lateralmente)
        GameObject targetNegozio = new GameObject("NegozioFocus");
        targetNegozio.transform.SetParent(targetsRoot.transform);
        targetNegozio.transform.localPosition = new Vector3(5f, 4f, -12f);
        targetNegozio.transform.localRotation = Quaternion.Euler(10f, -20f, 0f);

        // Target Impostazioni (Primo piano estremo dal basso, con futuro Depth of Field)
        GameObject targetImpostazioni = new GameObject("ImpostazioniFocus");
        targetImpostazioni.transform.SetParent(targetsRoot.transform);
        targetImpostazioni.transform.localPosition = new Vector3(0f, 2f, -8f);
        targetImpostazioni.transform.localRotation = Quaternion.Euler(5f, 0f, 0f);

        // Posizioniamo subito la camera sul focus dell'Officina
        cameraObj.transform.position = targetOfficina.transform.position;
        cameraObj.transform.rotation = targetOfficina.transform.rotation;

        // Aggiungiamo e configuriamo automaticamente lo script di regia
        HubCameraDirector director = cameraObj.AddComponent<HubCameraDirector>();
        director.officinaTarget = targetOfficina.transform;
        director.negozioTarget = targetNegozio.transform;
        director.impostazioniTarget = targetImpostazioni.transform;

        // --- Auto-Collegamento al GarageTransitionManager ---
        GarageTransitionManager transitionManager = Object.FindFirstObjectByType<GarageTransitionManager>();
        if (transitionManager != null)
        {
            transitionManager.garageInstance = garageRoot;
            
            // Crea un Transform vuoto esattamente in cima al piedistallo (Y = 0.5 è il centro, +0.5 è la cima)
            GameObject spawnPoint = new GameObject("PedestalSpawnPoint");
            spawnPoint.transform.SetParent(pedestal.transform);
            spawnPoint.transform.localPosition = new Vector3(0, 0.6f, 0); // Leggermente sopra
            spawnPoint.transform.localRotation = Quaternion.identity;
            
            transitionManager.garagePedestalPoint = spawnPoint.transform;
            Debug.Log("GarageTransitionManager aggiornato automaticamente coi nuovi riferimenti!");
        }

        Debug.Log("Garage 3D (Istanza Sotterranea) generato a Y = -5000!");
        
        // Segna la scena come modificata
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }

    private static void CreateWall(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.localPosition = pos;
        wall.transform.localScale = scale;
        wall.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }
}
#endif
