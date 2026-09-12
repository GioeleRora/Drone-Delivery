#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GarageUIBuilder : MonoBehaviour
{
    [MenuItem("Drone Delivery/Genera UI Garage Glassmorphism")]
    public static void GenerateGarageUI()
    {
        Debug.Log("Inizio generazione UI Garage Premium URP (GDD Compliant)...");

        GameObject gameManagerObj = GameObject.Find("GameManager");
        GarageTransitionManager gtm = gameManagerObj != null ? gameManagerObj.GetComponent<GarageTransitionManager>() : null;
        
        Camera garageCam = GameObject.Find("GarageCamera")?.GetComponent<Camera>();
        if (garageCam == null)
        {
            Debug.LogError("Non trovo la GarageCamera! Assicurati di averla nella scena.");
            return;
        }

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        // Creazione Materiale Glassmorphism
        Shader glassShader = Shader.Find("UI/URPGlassmorphism");
        Material glassMat = null;
        if (glassShader != null)
        {
            glassMat = new Material(glassShader);
            glassMat.SetColor("_Color", new Color(0.01f, 0.02f, 0.03f, 0.85f)); // Molto più scuro per contrastare le scritte
            glassMat.SetFloat("_BlurSize", 2.5f);
        }
        else
        {
            Debug.LogError("Shader UI/URPGlassmorphism non trovato! I pannelli non saranno sfocati.");
        }

        // --- FIX CAMERA TARGETS ---
        GameObject workbench = GameObject.Find("Workbench");
        if (workbench != null)
        {
            GameObject camTargets = GameObject.Find("CameraTargets");
            if (camTargets != null)
            {
                // Posiziona il target Officina davanti al tavolo
                Transform offTarget = camTargets.transform.Find("OfficinaTarget");
                if (offTarget != null)
                {
                    offTarget.position = workbench.transform.position + new Vector3(2f, 1.2f, 1.5f);
                    offTarget.LookAt(workbench.transform.position + new Vector3(0, 0.5f, 0));
                }
                
                // Posiziona Negozio a lato
                Transform negTarget = camTargets.transform.Find("NegozioTarget");
                if (negTarget != null)
                {
                    negTarget.position = workbench.transform.position + new Vector3(-2f, 1.5f, 1.5f);
                    negTarget.LookAt(workbench.transform.position + new Vector3(0, 0.5f, 0));
                }
                
                // Posiziona Impostazioni lontano per sfocare tutto
                Transform impTarget = camTargets.transform.Find("ImpostazioniTarget");
                if (impTarget != null)
                {
                    impTarget.position = workbench.transform.position + new Vector3(0f, 2f, 4f);
                    impTarget.LookAt(workbench.transform.position + new Vector3(0, 0.5f, 0));
                }
            }
        }

        GameObject canvasObj = GameObject.Find("HubCanvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("HubCanvas");
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        else
        {
            foreach (Transform child in canvasObj.transform) DestroyImmediate(child.gameObject);
        }

        // Setup ScreenSpace - Camera per URP Opaque Texture
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        if (canvas == null) canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = garageCam;
        canvas.planeDistance = 1.0f; // Vicino alla camera per non intersecare oggetti
        
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        if (gtm != null) gtm.hubCanvas = canvasObj;

        // Manager
        HubUIManager hubManager = canvasObj.GetComponent<HubUIManager>();
        if (hubManager == null) hubManager = canvasObj.AddComponent<HubUIManager>();

        Color textAccent = new Color(0.2f, 0.8f, 1f, 1f);
        Color textMuted = new Color(0.6f, 0.6f, 0.7f, 1f); // Grigio più chiaro

        // 1. ROOT PANEL
        RectTransform rootRect = CreateUIPanel("RootPanel", canvasObj.transform, new Color(0,0,0,0), null);
        rootRect.anchorMin = Vector2.zero; rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero; rootRect.offsetMax = Vector2.zero;

        // 2. FASCIA SUPERIORE
        RectTransform topBar = CreateUIPanel("TopBar", rootRect, Color.white, glassMat); // Il colore base è gestito dal materiale
        topBar.anchorMin = new Vector2(0, 0.9f); topBar.anchorMax = new Vector2(1, 1);
        topBar.offsetMin = new Vector2(40, -10); topBar.offsetMax = new Vector2(-40, -10);
        AddOutline(topBar.gameObject, new Color(0.3f, 0.7f, 1f, 0.5f));

        TextMeshProUGUI navText = AddTextToPanel(topBar, "NAV", Color.white, 28, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800, 50), TextAlignmentOptions.Center);
        hubManager.navText = navText;
        
        AddTextToPanel(topBar, "LIV 1   |   <color=#2CF>15.000 CR</color>", Color.white, 28, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-30, 0), new Vector2(400, 50), TextAlignmentOptions.Right);
        AddTextToPanel(topBar, "<b>DRONE DELIVERY</b>", Color.white, 32, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(30, 0), new Vector2(400, 50), TextAlignmentOptions.Left);

        // 3. FASCIA INFERIORE
        RectTransform bottomBar = CreateUIPanel("BottomBar", rootRect, new Color(0,0,0,0), null);
        bottomBar.anchorMin = new Vector2(0, 0); bottomBar.anchorMax = new Vector2(1, 0.1f);
        bottomBar.offsetMin = new Vector2(40, 10); bottomBar.offsetMax = new Vector2(-40, 10);

        AddTextToPanel(bottomBar, "(X) Seleziona    (O) Indietro", textMuted, 24, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-400, 0), new Vector2(400, 50), TextAlignmentOptions.Right);

        GameObject btnObj = new GameObject("Btn_Deploy");
        btnObj.transform.SetParent(bottomBar, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.05f, 0.1f, 0.15f, 0.9f); // Più scuro
        AddOutline(btnObj, textAccent);
        Button btn = btnObj.AddComponent<Button>();
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 0.5f); btnRect.anchorMax = new Vector2(1, 0.5f);
        btnRect.pivot = new Vector2(1, 0.5f);
        btnRect.anchoredPosition = new Vector2(0, 0); btnRect.sizeDelta = new Vector2(350, 60);

        AddTextToPanel(btnRect, "<color=#0FF>[ Triangolo ]</color> DEPLOY", Color.white, 28, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 50), TextAlignmentOptions.Center);
        if (gtm != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, gtm.ExecuteDeploy);

        // 4. PANNELLI SCHEDE (TABS)
        RectTransform tabsContainer = CreateUIPanel("TabsContainer", rootRect, new Color(0,0,0,0), null);
        tabsContainer.anchorMin = new Vector2(0, 0.15f); tabsContainer.anchorMax = new Vector2(1f, 0.85f);
        tabsContainer.offsetMin = Vector2.zero; tabsContainer.offsetMax = Vector2.zero;

        // TAB 1: OFFICINA
        RectTransform tabOfficina = CreateUIPanel("Tab_Officina", tabsContainer, new Color(0,0,0,0), null);
        tabOfficina.anchorMin = Vector2.zero; tabOfficina.anchorMax = Vector2.one;
        tabOfficina.offsetMin = Vector2.zero; tabOfficina.offsetMax = Vector2.zero;
        hubManager.officinaPanel = tabOfficina.gameObject;

        RectTransform leftPanel = CreateUIPanel("ListaComponenti", tabOfficina, Color.white, glassMat);
        leftPanel.anchorMin = new Vector2(0, 0); leftPanel.anchorMax = new Vector2(0.3f, 1f);
        leftPanel.offsetMin = new Vector2(40, 0); leftPanel.offsetMax = new Vector2(0, 0);
        AddOutline(leftPanel.gameObject, new Color(0.3f, 0.7f, 1f, 0.5f));
        AddDropShadow(leftPanel.gameObject);

        AddTextToPanel(leftPanel, "COMPONENTI", textAccent, 32, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(300, 50), TextAlignmentOptions.Center);
        // Testi più chiari
        string fakeList = "<color=#FFF>> MOTORI (Liv. 2)</color>\n\n<color=#CCC>  BATTERIA (Liv. 1)</color>\n\n<color=#CCC>  TELAIO (Liv. 1)</color>\n\n<color=#CCC>  MODULI EXTRA</color>";
        AddTextToPanel(leftPanel, fakeList, Color.white, 24, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(40, -100), new Vector2(-80, 400), TextAlignmentOptions.TopLeft);

        RectTransform rightPanel = CreateUIPanel("RadarStats", tabOfficina, Color.white, glassMat);
        rightPanel.anchorMin = new Vector2(0.7f, 0); rightPanel.anchorMax = new Vector2(1f, 1f);
        rightPanel.offsetMin = new Vector2(0, 0); rightPanel.offsetMax = new Vector2(-40, 0);
        AddOutline(rightPanel.gameObject, new Color(0.3f, 0.7f, 1f, 0.5f));
        AddDropShadow(rightPanel.gameObject);

        AddTextToPanel(rightPanel, "STATISTICHE", textAccent, 32, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(300, 50), TextAlignmentOptions.Center);
        string fakeStats = "Spinta:\t\t<color=#2CF>140N</color>\nPeso:\t\t<color=#F44>45Kg</color>\nIntegrità:\t<color=#2CF>100%</color>";
        AddTextToPanel(rightPanel, fakeStats, Color.white, 24, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(40, 50), new Vector2(-80, 150), TextAlignmentOptions.BottomLeft);

        // TAB 2: NEGOZIO
        RectTransform tabNegozio = CreateUIPanel("Tab_Negozio", tabsContainer, new Color(0,0,0,0), null);
        tabNegozio.anchorMin = Vector2.zero; tabNegozio.anchorMax = Vector2.one;
        tabNegozio.offsetMin = Vector2.zero; tabNegozio.offsetMax = Vector2.zero;
        hubManager.negozioPanel = tabNegozio.gameObject;
        
        RectTransform storePanel = CreateUIPanel("StoreContent", tabNegozio, Color.white, glassMat);
        storePanel.anchorMin = new Vector2(0.2f, 0); storePanel.anchorMax = new Vector2(0.8f, 1f);
        storePanel.offsetMin = Vector2.zero; storePanel.offsetMax = Vector2.zero;
        AddOutline(storePanel.gameObject, new Color(0.3f, 0.7f, 1f, 0.5f));
        AddTextToPanel(storePanel, "MERCATO NERO\n<size=20><color=#CCC>Connettiti alla rete ribelle per acquistare nuovi moduli</color></size>", Color.white, 36, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800, 200), TextAlignmentOptions.Center);
        tabNegozio.gameObject.SetActive(false); // Nascondi all'inizio

        // TAB 3: IMPOSTAZIONI
        RectTransform tabImpostazioni = CreateUIPanel("Tab_Impostazioni", tabsContainer, new Color(0,0,0,0), null);
        tabImpostazioni.anchorMin = Vector2.zero; tabImpostazioni.anchorMax = Vector2.one;
        tabImpostazioni.offsetMin = Vector2.zero; tabImpostazioni.offsetMax = Vector2.zero;
        hubManager.impostazioniPanel = tabImpostazioni.gameObject;
        
        RectTransform settingsPanel = CreateUIPanel("SettingsContent", tabImpostazioni, Color.white, glassMat);
        settingsPanel.anchorMin = new Vector2(0.3f, 0.2f); settingsPanel.anchorMax = new Vector2(0.7f, 0.8f);
        settingsPanel.offsetMin = Vector2.zero; settingsPanel.offsetMax = Vector2.zero;
        AddOutline(settingsPanel.gameObject, new Color(0.3f, 0.7f, 1f, 0.5f));
        AddTextToPanel(settingsPanel, "IMPOSTAZIONI DI SISTEMA", textAccent, 32, new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 50), TextAlignmentOptions.Center);
        tabImpostazioni.gameObject.SetActive(false); // Nascondi all'inizio

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("<color=cyan>UI Garage True-Glassmorphism generata con successo!</color>");
    }

    private static RectTransform CreateUIPanel(string name, Transform parent, Color color, Material mat)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.color = color;
        if (mat != null) img.material = mat;
        return obj.GetComponent<RectTransform>();
    }

    private static void AddOutline(GameObject obj, Color color)
    {
        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(1, -1);
    }

    private static void AddDropShadow(GameObject obj)
    {
        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.7f);
        shadow.effectDistance = new Vector2(4, -4);
    }

    private static TextMeshProUGUI AddTextToPanel(RectTransform parent, string text, Color color, int size, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 sizeDelta, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = alignment;
        
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = pos;
        rect.sizeDelta = sizeDelta;
        return tmp;
    }
}
#endif
