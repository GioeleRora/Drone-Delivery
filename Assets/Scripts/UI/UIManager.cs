using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Tooltip("Il Transform del drone da cui leggere l'altitudine")]
    [SerializeField] private Transform droneTransform;
    
    [Tooltip("Il testo dell'interfaccia in cui mostrare il valore dell'altitudine")]
    [SerializeField] private TextMeshProUGUI altitudeText;

    [Header("Motors Status")]
    [Tooltip("Riferimento allo script di movimento del drone")]
    [SerializeField] private DroneMovement droneMovement;

    [SerializeField] private UnityEngine.UI.Image batteryFill;
    [SerializeField] private TextMeshProUGUI batteryPercentage;

    [Header("Game State Panels")]
    [Tooltip("Pannello mostrato in caso di crash del drone")]
    [SerializeField] private GameObject gameOverCrashPanel;

    [Tooltip("Pannello mostrato in caso di batteria scarica")]
    [SerializeField] private GameObject gameOverBatteryPanel;

    [Tooltip("Pannello mostrato in caso di vittoria (quota consegne raggiunta)")]
    [SerializeField] private GameObject victoryPanel;

    private void Awake()
    {
        // Se i pannelli non sono assegnati nell'Inspector, li generiamo automaticamente a runtime
        if (gameOverCrashPanel == null)
        {
            gameOverCrashPanel = CreatePanel("GameOverCrashPanel", "DRONE DISTRUTTO!", Color.red, "La collisione è stata troppo violenta.");
        }
        if (gameOverBatteryPanel == null)
        {
            gameOverBatteryPanel = CreatePanel("GameOverBatteryPanel", "BATTERIA ESAURITA!", Color.yellow, "I motori si sono spenti a mezz'aria.");
        }
        if (victoryPanel == null)
        {
            victoryPanel = CreatePanel("VictoryPanel", "CONSEGNE COMPLETATE!", Color.green, "Tutti i pacchi sono stati consegnati con successo.");
        }
    }

    private void ShowPanel(GameObject panel)
    {
        if (gameOverCrashPanel != null) gameOverCrashPanel.SetActive(false);
        if (gameOverBatteryPanel != null) gameOverBatteryPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        if (panel != null) panel.SetActive(true);
        Time.timeScale = 0f;
    }

    private GameObject CreatePanel(string name, string titleText, Color titleColor, string subtitleText)
    {
        // 1. Crea il GameObject principale del pannello
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
        panel.transform.SetParent(this.transform, false);

        // Imposta l'ancoraggio per riempire tutto lo schermo (Stretch-Stretch)
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        // Colore di sfondo scuro semi-trasparente
        UnityEngine.UI.Image img = panel.GetComponent<UnityEngine.UI.Image>();
        img.color = new Color(0f, 0f, 0f, 0.75f);

        // 2. Crea il Testo del Titolo (TextMeshProUGUI)
        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(panel.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0f, 80f);
        titleRect.sizeDelta = new Vector2(600f, 80f);
        TextMeshProUGUI titleTMP = titleObj.GetComponent<TextMeshProUGUI>();
        titleTMP.text = titleText;
        titleTMP.fontSize = 42f;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = titleColor;
        titleTMP.alignment = TextAlignmentOptions.Center;

        // 3. Crea il Testo del Sottotitolo
        GameObject subObj = new GameObject("Subtitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        subObj.transform.SetParent(panel.transform, false);
        RectTransform subRect = subObj.GetComponent<RectTransform>();
        subRect.anchoredPosition = new Vector2(0f, 10f);
        subRect.sizeDelta = new Vector2(600f, 50f);
        TextMeshProUGUI subTMP = subObj.GetComponent<TextMeshProUGUI>();
        subTMP.text = subtitleText;
        subTMP.fontSize = 20f;
        subTMP.color = Color.white;
        subTMP.alignment = TextAlignmentOptions.Center;

        // 4. Crea il Pulsante di Riavvio (Restart Button)
        GameObject btnObj = new GameObject("RestartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
        btnObj.transform.SetParent(panel.transform, false);
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0f, -80f);
        btnRect.sizeDelta = new Vector2(180f, 50f);

        // Colore verde neon per il pulsante
        UnityEngine.UI.Image btnImg = btnObj.GetComponent<UnityEngine.UI.Image>();
        btnImg.color = new Color(0.1f, 0.8f, 0.4f, 1f);

        UnityEngine.UI.Button btn = btnObj.GetComponent<UnityEngine.UI.Button>();
        btn.onClick.AddListener(OnRestartButtonClicked);

        // Testo del Pulsante
        GameObject btnTextObj = new GameObject("ButtonText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        btnTextObj.transform.SetParent(btnObj.transform, false);
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.sizeDelta = new Vector2(180f, 50f);
        TextMeshProUGUI btnTextTMP = btnTextObj.GetComponent<TextMeshProUGUI>();
        btnTextTMP.text = "RIAVVIA";
        btnTextTMP.fontSize = 18f;
        btnTextTMP.fontStyle = FontStyles.Bold;
        btnTextTMP.color = Color.black;
        btnTextTMP.alignment = TextAlignmentOptions.Center;

        return panel;
    }

    private void Start()
    {
        // Disattivazione iniziale dei pannelli di gioco finito
        if (gameOverCrashPanel != null) gameOverCrashPanel.SetActive(false);
        if (gameOverBatteryPanel != null) gameOverBatteryPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        if (droneMovement != null)
        {
            DroneHealth health = droneMovement.GetComponent<DroneHealth>();
            if (health != null) health.OnDroneDestroyed += () => ShowPanel(gameOverCrashPanel);
            
            BatterySystem battery = droneMovement.GetComponent<BatterySystem>();
            if (battery != null) battery.OnBatteryDepleted += () => ShowPanel(gameOverBatteryPanel);
        }
    }

    public void OnRestartButtonClicked()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        // Controllo di sicurezza per evitare NullReferenceException sull'altitudine
        if (droneTransform != null && altitudeText != null)
        {
            int altitude = Mathf.RoundToInt(droneTransform.position.y);
            altitudeText.text = "Altitudine: " + altitude + " m";
        }

        // Controllo della batteria
        if (droneMovement != null && batteryFill != null && batteryPercentage != null)
        {
            BatterySystem battery = droneMovement.GetComponent<BatterySystem>();
            if (battery != null)
            {
                float fill = battery.CurrentBattery / battery.MaxBattery;
                batteryFill.fillAmount = fill;
                batteryFill.color = Color.Lerp(Color.red, Color.green, fill);
                batteryPercentage.text = Mathf.RoundToInt(fill * 100) + "%";
            }
        }
    }

    public void OnToggleMotorsClicked()
    {
        if (droneMovement != null)
        {
            droneMovement.ToggleMotors();
        }
    }
}
