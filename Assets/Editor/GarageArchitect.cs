#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class GarageArchitect : MonoBehaviour
{
    [MenuItem("Drone Delivery/Rimodella Stanza Segreta")]
    public static void RemodelGarage()
    {
        Debug.Log("Rimodellazione Garage in Stanzetta Segreta...");

        GameObject garage = GameObject.Find("GarageInstance");
        if (garage == null)
        {
            Debug.LogError("Non trovo GarageInstance");
            return;
        }

        // Ridimensiona i muri a una stanza 6x6, alta 3 metri
        Transform floor = garage.transform.Find("Floor");
        if (floor != null) { floor.localScale = new Vector3(6, 1, 6); floor.localPosition = new Vector3(0, -0.5f, 0); }
        
        Transform ceiling = garage.transform.Find("Ceiling");
        if (ceiling != null) { ceiling.localScale = new Vector3(6, 1, 6); ceiling.localPosition = new Vector3(0, 3.5f, 0); }

        Transform wallN = garage.transform.Find("Wall_North");
        if (wallN != null) { wallN.localScale = new Vector3(6, 3, 1); wallN.localPosition = new Vector3(0, 1.5f, 3.5f); }

        Transform wallS = garage.transform.Find("Wall_South");
        if (wallS != null) { wallS.localScale = new Vector3(6, 3, 1); wallS.localPosition = new Vector3(0, 1.5f, -3.5f); }

        Transform wallE = garage.transform.Find("Wall_East");
        if (wallE != null) { wallE.localScale = new Vector3(1, 3, 6); wallE.localPosition = new Vector3(3.5f, 1.5f, 0); }

        // La porta a ovest
        Transform wallWL = garage.transform.Find("Wall_West_L");
        if (wallWL != null) { wallWL.localScale = new Vector3(1, 3, 2); wallWL.localPosition = new Vector3(-3.5f, 1.5f, -2); }

        Transform wallWR = garage.transform.Find("Wall_West_R");
        if (wallWR != null) { wallWR.localScale = new Vector3(1, 3, 2); wallWR.localPosition = new Vector3(-3.5f, 1.5f, 2); }

        Transform wallWT = garage.transform.Find("Wall_West_Top");
        if (wallWT != null) { wallWT.localScale = new Vector3(1, 1, 2); wallWT.localPosition = new Vector3(-3.5f, 2.5f, 0); }

        Transform door = garage.transform.Find("GarageDoor");
        if (door != null) { door.localScale = new Vector3(0.2f, 2, 2); door.localPosition = new Vector3(-3.5f, 1f, 0); }

        // Trova il piedistallo e ridimensionalo a "Tavolo da lavoro"
        GameObject pedestal = GameObject.Find("GaragePedestal");
        if (pedestal != null)
        {
            pedestal.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // Dimezzato
            pedestal.transform.localPosition = new Vector3(0, 0, 0); // Centro stanza
            
            // Sistema il punto di spawn
            Transform spawn = pedestal.transform.Find("SpawnPoint");
            if (spawn != null) spawn.localPosition = new Vector3(0, 1.5f, 0); // Alzato per il tavolo
        }

        // Sistema la telecamera
        GameObject cam = GameObject.Find("GarageCamera");
        if (cam != null && pedestal != null)
        {
            cam.transform.position = pedestal.transform.position + new Vector3(1.5f, 1f, 1.5f);
            cam.transform.LookAt(pedestal.transform.position + new Vector3(0, 0.5f, 0));
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("<color=green>Stanzetta completata!</color>");
    }
}
#endif
