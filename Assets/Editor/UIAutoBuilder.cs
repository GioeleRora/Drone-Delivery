using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIAutoBuilder : EditorWindow
{
    [MenuItem("Drone Tools/Genera UI Automatica")]
    public static void GenerateUI()
    {
        // 1. Pulizia Vecchia UI
        Canvas[] existingCanvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude);
        foreach (var c in existingCanvases) { DestroyImmediate(c.gameObject); }
        EventSystem[] existingEventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Exclude);
        foreach (var ev in existingEventSystems) { DestroyImmediate(ev.gameObject); }

        // 2. Creazione Canvas Base
        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<StandaloneInputModule>();

        DroneMovement drone = Object.FindAnyObjectByType<DroneMovement>(FindObjectsInactive.Include);
        
        if (drone != null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                CameraFollow camFollow = mainCam.GetComponent<CameraFollow>();
                if (camFollow == null)
                {
                    camFollow = mainCam.gameObject.AddComponent<CameraFollow>();
                }
                camFollow.target = drone.transform;
            }
        }
        
        GameObject uiManagerGo = new GameObject("UIManager");
        uiManagerGo.transform.SetParent(canvasGo.transform, false);
        UIManager uiManager = uiManagerGo.AddComponent<UIManager>();

        Sprite knobSprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        Sprite backgroundSprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");

        // ============================================
        // A. Pannelli HUD (Alto a Sinistra)
        // ============================================
        GameObject hudPanelGo = new GameObject("HUDPanel");
        hudPanelGo.transform.SetParent(canvasGo.transform, false);
        RectTransform hudPanelRect = hudPanelGo.AddComponent<RectTransform>();
        hudPanelRect.anchorMin = new Vector2(0, 1);
        hudPanelRect.anchorMax = new Vector2(0, 1);
        hudPanelRect.pivot = new Vector2(0, 1);
        hudPanelRect.anchoredPosition = new Vector2(20, -20);
        hudPanelRect.sizeDelta = new Vector2(400, 200);

        // -- BatteryBox --
        GameObject batteryBoxGo = new GameObject("BatteryBox");
        batteryBoxGo.transform.SetParent(hudPanelGo.transform, false);
        Image batBoxImg = batteryBoxGo.AddComponent<Image>();
        batBoxImg.color = new Color(0, 0, 0, 0.7f);
        RectTransform batBoxRect = batBoxImg.rectTransform;
        batBoxRect.anchorMin = new Vector2(0, 1);
        batBoxRect.anchorMax = new Vector2(0, 1);
        batBoxRect.pivot = new Vector2(0, 1);
        batBoxRect.anchoredPosition = new Vector2(0, 0);
        batBoxRect.sizeDelta = new Vector2(350, 60);

        GameObject batIconGo = new GameObject("BatteryIcon");
        batIconGo.transform.SetParent(batteryBoxGo.transform, false);
        Image batIcon = batIconGo.AddComponent<Image>();
        batIcon.sprite = backgroundSprite;
        RectTransform batIconRect = batIcon.rectTransform;
        batIconRect.anchorMin = new Vector2(0, 0.5f);
        batIconRect.anchorMax = new Vector2(0, 0.5f);
        batIconRect.pivot = new Vector2(0, 0.5f);
        batIconRect.anchoredPosition = new Vector2(10, 0);
        batIconRect.sizeDelta = new Vector2(40, 40);

        GameObject batFillContainerGo = new GameObject("BatteryFillContainer");
        batFillContainerGo.transform.SetParent(batteryBoxGo.transform, false);
        Image batFillContainerBg = batFillContainerGo.AddComponent<Image>();
        batFillContainerBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        RectTransform batFillContainerRect = batFillContainerBg.rectTransform;
        batFillContainerRect.anchorMin = new Vector2(0, 0.5f);
        batFillContainerRect.anchorMax = new Vector2(0, 0.5f);
        batFillContainerRect.pivot = new Vector2(0, 0.5f);
        batFillContainerRect.anchoredPosition = new Vector2(60, 0);
        batFillContainerRect.sizeDelta = new Vector2(200, 30);

        GameObject batFillGo = new GameObject("Fill");
        batFillGo.transform.SetParent(batFillContainerGo.transform, false);
        Image batteryFill = batFillGo.AddComponent<Image>();
        batteryFill.color = Color.green;
        batteryFill.sprite = backgroundSprite;
        batteryFill.type = Image.Type.Filled;
        batteryFill.fillMethod = Image.FillMethod.Horizontal;
        RectTransform fillRect = batteryFill.rectTransform;
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        GameObject batTextGo = new GameObject("PercentageText");
        batTextGo.transform.SetParent(batteryBoxGo.transform, false);
        TextMeshProUGUI batText = batTextGo.AddComponent<TextMeshProUGUI>();
        batText.text = "100%";
        batText.fontSize = 24;
        batText.color = Color.white;
        batText.fontStyle = FontStyles.Bold;
        batText.alignment = TextAlignmentOptions.Right;
        RectTransform batTextRect = batText.rectTransform;
        batTextRect.anchorMin = new Vector2(1, 0.5f);
        batTextRect.anchorMax = new Vector2(1, 0.5f);
        batTextRect.pivot = new Vector2(1, 0.5f);
        batTextRect.anchoredPosition = new Vector2(-10, 0);
        batTextRect.sizeDelta = new Vector2(80, 50);

        // -- AltitudeBox --
        GameObject altBoxGo = new GameObject("AltitudeBox");
        altBoxGo.transform.SetParent(hudPanelGo.transform, false);
        Image altBoxImg = altBoxGo.AddComponent<Image>();
        altBoxImg.color = new Color(0, 0, 0, 0.7f);
        RectTransform altBoxRect = altBoxImg.rectTransform;
        altBoxRect.anchorMin = new Vector2(0, 1);
        altBoxRect.anchorMax = new Vector2(0, 1);
        altBoxRect.pivot = new Vector2(0, 1);
        altBoxRect.anchoredPosition = new Vector2(0, -70);
        altBoxRect.sizeDelta = new Vector2(350, 60);

        GameObject altTextGo = new GameObject("AltitudeText");
        altTextGo.transform.SetParent(altBoxGo.transform, false);
        TextMeshProUGUI altText = altTextGo.AddComponent<TextMeshProUGUI>();
        altText.text = "ALT: 0 m";
        altText.fontSize = 28;
        altText.color = Color.white;
        altText.fontStyle = FontStyles.Bold;
        altText.alignment = TextAlignmentOptions.Left;
        RectTransform altRect = altText.rectTransform;
        altRect.anchorMin = new Vector2(0, 0.5f);
        altRect.anchorMax = new Vector2(1, 0.5f);
        altRect.pivot = new Vector2(0, 0.5f);
        altRect.anchoredPosition = new Vector2(20, 0);
        altRect.sizeDelta = new Vector2(-40, 50);

        SerializedObject uiManagerSO = new SerializedObject(uiManager);
        uiManagerSO.FindProperty("droneTransform").objectReferenceValue = drone != null ? drone.transform : null;
        uiManagerSO.FindProperty("altitudeText").objectReferenceValue = altText;
        uiManagerSO.FindProperty("droneMovement").objectReferenceValue = drone;
        uiManagerSO.FindProperty("batteryFill").objectReferenceValue = batteryFill;
        uiManagerSO.FindProperty("batteryPercentage").objectReferenceValue = batText;
        uiManagerSO.ApplyModifiedProperties();

        // ============================================
        // B. Joystick rimossi per transizione a PC
        // ============================================

        // ============================================
        // C. Bussola a Nastro (Basso Centro)
        // ============================================
        GameObject compassBoxGo = new GameObject("CompassBox");
        compassBoxGo.transform.SetParent(canvasGo.transform, false);
        Image compassImg = compassBoxGo.AddComponent<Image>();
        compassImg.color = new Color(0, 0, 0, 0.8f);
        compassBoxGo.AddComponent<RectMask2D>(); // Nasconde il testo che sfora i bordi
        RectTransform compassBoxRect = compassImg.rectTransform;
        compassBoxRect.anchorMin = new Vector2(0.5f, 0);
        compassBoxRect.anchorMax = new Vector2(0.5f, 0);
        compassBoxRect.pivot = new Vector2(0.5f, 0);
        compassBoxRect.anchoredPosition = new Vector2(0, 50);
        compassBoxRect.sizeDelta = new Vector2(400, 40);

        GameObject compassTapeGo = new GameObject("CompassTape");
        compassTapeGo.transform.SetParent(compassBoxGo.transform, false);
        TextMeshProUGUI tapeText = compassTapeGo.AddComponent<TextMeshProUGUI>();
        // Nastro esteso per permettere rotazioni multiple (può essere ottimizzato con font monospazio o sprite)
        tapeText.text = "|  |  N  |  |  E  |  |  S  |  |  W  |  |  N  |  |  E  |  |  S  |  |  W  |  |  N  |  |";
        tapeText.fontSize = 24;
        tapeText.color = Color.white;
        tapeText.alignment = TextAlignmentOptions.Center;
        tapeText.textWrappingMode = TextWrappingModes.NoWrap;
        RectTransform tapeRect = tapeText.rectTransform;
        tapeRect.anchorMin = new Vector2(0.5f, 0.5f);
        tapeRect.anchorMax = new Vector2(0.5f, 0.5f);
        tapeRect.pivot = new Vector2(0.5f, 0.5f);
        tapeRect.anchoredPosition = Vector2.zero;
        tapeRect.sizeDelta = new Vector2(2000, 40);

        CompassHUD compassHUD = compassBoxGo.AddComponent<CompassHUD>();
        SerializedObject compassSO = new SerializedObject(compassHUD);
        compassSO.FindProperty("drone").objectReferenceValue = drone != null ? drone.transform : null;
        compassSO.FindProperty("compassTape").objectReferenceValue = tapeRect;
        compassSO.FindProperty("pixelsPerDegree").floatValue = 2f;
        compassSO.ApplyModifiedProperties();

        // ============================================
        // D. Interruttore Motori (Alto a Destra)
        // ============================================
        GameObject toggleGo = new GameObject("AnimatedToggle");
        toggleGo.transform.SetParent(canvasGo.transform, false);
        RectTransform toggleRect = toggleGo.AddComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(1, 1);
        toggleRect.anchorMax = new Vector2(1, 1);
        toggleRect.pivot = new Vector2(1, 1);
        toggleRect.anchoredPosition = new Vector2(-80f, -60f);
        toggleRect.sizeDelta = new Vector2(120f, 60f);

        Image toggleBgImg = toggleGo.AddComponent<Image>();
        toggleBgImg.color = Color.red;
        toggleBgImg.sprite = backgroundSprite;
        toggleBgImg.type = Image.Type.Sliced;
        toggleBgImg.pixelsPerUnitMultiplier = 4f; // Forza i bordi ad arrotondarsi per ricreare la pillola

        Button toggleBtn = toggleGo.AddComponent<Button>();
        toggleBtn.navigation = new Navigation { mode = Navigation.Mode.None };

        GameObject handleGo = new GameObject("Handle");
        handleGo.transform.SetParent(toggleGo.transform, false);
        Image handleImage = handleGo.AddComponent<Image>();
        handleImage.sprite = knobSprite;
        handleImage.type = Image.Type.Simple;
        handleImage.color = Color.white;
        RectTransform handleRect = handleGo.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(50f, 50f); // Proporzionato al box di 60
        handleRect.anchoredPosition = new Vector2(-30f, 0); // Spostato a sinistra inizialmente

        AnimatedToggle animToggle = toggleGo.AddComponent<AnimatedToggle>();
        
        SerializedObject animToggleSO = new SerializedObject(animToggle);
        animToggleSO.FindProperty("droneMovement").objectReferenceValue = drone;
        animToggleSO.FindProperty("handle").objectReferenceValue = handleRect;
        animToggleSO.FindProperty("backgroundImage").objectReferenceValue = toggleBgImg;
        animToggleSO.ApplyModifiedProperties();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(toggleBtn.onClick, new UnityEngine.Events.UnityAction(animToggle.OnToggleClicked));

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("UI Mobile generata con Joystick e Bussola a Nastro!");
    }

}
